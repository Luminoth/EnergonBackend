using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;

using log4net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class ResponseMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ResponseMessageHandler));

        internal ResponseMessageHandler()
        {
        }

// TODO: handle the case of a user authenticating a second time

        private async Task CompleteAuthentication(AuthSession session)
        {
            SessionId sessionid = new SessionId(ConfigurationManager.AppSettings["sessionSecret"]);
            Logger.Info("Session " + session.Id + " generated sessionid '" + sessionid.SessionID + "' for account '" + session.AccountInfo.Username + "'");
            await session.Success(sessionid.SessionID);
        }

        private async Task Authenticate(AuthSession session, string username, string nonce, string cnonce, string nc, string qop, string digestURI, string response)
        {
            AccountInfo account = new AccountInfo() { Username = username };
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                if(!await account.Read(connection)) {
                    await session.Failure("Bad Username or Password");
                    return;
                }
            }

            if(!account.Active) {
                await session.Failure("Account Inactive");
                return;
            }

            string expected = string.Empty, rspauth = string.Empty;
            switch(session.AuthType)
            {
            /*case Heroes.Core.AuthType.DigestMD5:
                Logger.Debug("Handling MD5 response...");
                //Logger.Debug("passwordHash=" + account.PasswordMD5);
                expected = EnergonSoftware.Core.Auth.DigestClientResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI);
                rspauth = EnergonSoftware.Core.Auth.DigestServerResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI);
                break;*/
            case EnergonSoftware.Core.AuthType.DigestSHA512:
                Logger.Debug("Handling SHA512 response...");
                //Logger.Debug("passwordHash=" + account.PasswordSHA512);
                expected = EnergonSoftware.Core.Auth.DigestClientResponse(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI);
                rspauth = EnergonSoftware.Core.Auth.DigestServerResponse(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI);
                break;
            default:
                await session.Failure("Unsupported Auth Type");
                return;
            }

            if(!expected.Equals(response)) {
                await session.Failure("Bad Username or Password");
                return;
            }

            string challenge = "rspauth=" + rspauth;
            Logger.Info("Session " + session.Id + " authenticated account '" + username + "', sending response: " + rspauth);
            session.Challenge(challenge, account);
        }

        protected async override void OnHandleMessage(IMessage message, Session session)
        {
            AuthSession authSession = (AuthSession)session;

            if(authSession.Authenticated) {
                await CompleteAuthentication(authSession);
                return;
            }

            if(!authSession.Authenticating) {
                await authSession.Failure("Not Authenticating");
                return;
            }

            if(authSession.AuthNonce.Expired) {
                await authSession.Failure("Session Expired");
                return;
            }

            ResponseMessage response = (ResponseMessage)message;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response.Response));
            Logger.Debug("Decoded response: " + decoded);

            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(decoded);
            if(null == values || 0 == values.Count) {
                await authSession.Failure("Invalid Response");
                return;
            }

            try {
                string username = values["username"].Trim(new char[] { '"' });
                await EventLogger.Instance.BeginEvent(authSession.RemoteEndPoint, username);

                string charset = values["charset"].Trim(new char[] { '"' });
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.Failure("Invalid Response");
                    return;
                }

                string qop = values["qop"].Trim(new char[] { '"' });
                if(!"auth".Equals(qop, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.Failure("Invalid Response");
                    return;
                }

                string realm = values["realm"].Trim(new char[] { '"' });
                if(!ConfigurationManager.AppSettings["authRealm"].Equals(realm, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.Failure("Invalid Response");
                    return;
                }

                string nonce = values["nonce"].Trim(new char[] { '"' });
                if(!authSession.AuthNonce.NonceHash.Equals(nonce)) {
                    await authSession.Failure("Invalid Response");
                    return;
                }

                string digestURI = values["digest-uri"].Trim(new char[] { '"' });
                // TODO: validate the digest-uri

                string cnonce = values["cnonce"].Trim(new char[] { '"' });
                string nc = values["nc"].Trim(new char[] { '"' });
                string rsp = values["response"].Trim(new char[] { '"' });

                await Authenticate(authSession, username, nonce, cnonce, nc, qop, digestURI, rsp);
            } catch(KeyNotFoundException) {
                Task.Run(() => authSession.Failure("Invalid response!")).Wait();
            }
        }
    }
}
