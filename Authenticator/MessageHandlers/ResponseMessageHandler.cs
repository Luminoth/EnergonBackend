using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;

using log4net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class ResponseMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ResponseMessageHandler));

        private readonly AuthSession _session;

        internal ResponseMessageHandler(AuthSession session)
        {
            _session = session;
        }

// TODO: handle the case of a user authenticating a second time

        private async Task CompleteAuthentication()
        {
            SessionId sessionid = new SessionId(ConfigurationManager.AppSettings["sessionSecret"]);
            Logger.Info("Session " + _session.Id + " generated sessionid '" + sessionid.SessionID + "' for account '" + _session.AccountInfo.Username + "'");
            await _session.Success(sessionid.SessionID);
        }

        private async Task Authenticate(string username, string nonce, string cnonce, string nc, string qop, string digestURI, string response)
        {
            AccountInfo account = new AccountInfo(username);
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                if(!await account.Read(connection)) {
                    await _session.Failure("Bad Username or Password");
                    return;
                }
            }

            if(!account.Active) {
                await _session.Failure("Account Inactive");
                return;
            }

            string expected = string.Empty, rspauth = string.Empty;
            switch(_session.AuthType)
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
                await _session.Failure("Unsupported Auth Type");
                return;
            }

            if(!expected.Equals(response)) {
                await _session.Failure("Bad Username or Password");
                return;
            }

            string challenge = "rspauth=" + rspauth;
            Logger.Info("Session " + _session.Id + " authenticated account '" + username + "', sending response: " + rspauth);
            _session.Challenge(challenge, account);
        }

        protected async override void OnHandleMessage(IMessage message)
        {
            if(_session.Authenticated) {
                await CompleteAuthentication();
                return;
            }

            if(!_session.Authenticating) {
                await _session.Failure("Not Authenticating");
                return;
            }

            if(_session.AuthNonce.Expired) {
                await _session.Failure("Session Expired");
                return;
            }

            ResponseMessage response = (ResponseMessage)message;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response.Response));
            Logger.Debug("Decoded response: " + decoded);

            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(decoded);
            if(null == values || 0 == values.Count) {
                await _session.Failure("Invalid Response");
                return;
            }

            try {
                string username = values["username"].Trim(new char[] { '"' });
                await EventLogger.Instance.BeginEvent(_session.RemoteEndPoint, username);

                string charset = values["charset"].Trim(new char[] { '"' });
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    await _session.Failure("Invalid Response");
                    return;
                }

                string qop = values["qop"].Trim(new char[] { '"' });
                if(!"auth".Equals(qop, StringComparison.InvariantCultureIgnoreCase)) {
                    await _session.Failure("Invalid Response");
                    return;
                }

                string realm = values["realm"].Trim(new char[] { '"' });
                if(!ConfigurationManager.AppSettings["authRealm"].Equals(realm, StringComparison.InvariantCultureIgnoreCase)) {
                    await _session.Failure("Invalid Response");
                    return;
                }

                string nonce = values["nonce"].Trim(new char[] { '"' });
                if(!_session.AuthNonce.NonceHash.Equals(nonce)) {
                    await _session.Failure("Invalid Response");
                    return;
                }

                string digestURI = values["digest-uri"].Trim(new char[] { '"' });
                // TODO: validate the digest-uri

                string cnonce = values["cnonce"].Trim(new char[] { '"' });
                string nc = values["nc"].Trim(new char[] { '"' });
                string rsp = values["response"].Trim(new char[] { '"' });

                await Authenticate(username, nonce, cnonce, nc, qop, digestURI, rsp);
            } catch(KeyNotFoundException) {
                Task.Run(() => _session.Failure("Invalid response!")).Wait();
            }
        }
    }
}
