using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Net;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class ResponseMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ResponseMessageHandler));

        internal ResponseMessageHandler()
        {
        }

        //// TODO: handle the case of a user authenticating a second time

        private async Task CompleteAuthenticationAsync(AuthSession session)
        {
            SessionId sessionid = new SessionId(ConfigurationManager.AppSettings["sessionSecret"]);
            Logger.Info("Session " + session.Id + " generated sessionid '" + sessionid.SessionID + "' for account '" + session.AccountInfo.AccountName + "'");
            await session.SuccessAsync(sessionid.SessionID).ConfigureAwait(false);
        }

        private async Task AuthenticateAsync(AuthSession session, string accountName, string nonce, string cnonce, string nc, string qop, string digestURI, string response)
        {
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(accounts.Count() < 1) {
                    await session.FailureAsync("Bad Username or Password").ConfigureAwait(false);
                    return;
                }

                AccountInfo account = accounts.First();
                if(!account.IsActive) {
                    await session.FailureAsync("Account Inactive").ConfigureAwait(false);
                    return;
                }

                string expected = string.Empty, rspauth = string.Empty;
                switch(session.AuthType)
                {
                case AuthType.DigestMD5:
                    /*Logger.Debug("Handling MD5 response...");
                    ////Logger.Debug("passwordHash=" + account.PasswordMD5);
                    expected = await AuthUtil.DigestClientResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI).ConfigureAwait(false);
                    rspauth = await AuthUtil.DigestServerResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI).ConfigureAwait(false);
                    break;*/
await session.FailureAsync("MD5 auth type not supported!").ConfigureAwait(false);
return;
                case AuthType.DigestSHA512:
                    Logger.Debug("Handling SHA512 response...");
                    ////Logger.Debug("passwordHash=" + account.PasswordSHA512);
                    expected = await AuthUtil.DigestClientResponseAsync(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI).ConfigureAwait(false);
                    rspauth = await AuthUtil.DigestServerResponseAsync(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI).ConfigureAwait(false);
                    break;
                default:
                    await session.FailureAsync("Unsupported auth type!").ConfigureAwait(false);
                    return;
                }

                if(!expected.Equals(response)) {
                    await session.FailureAsync("Bad Username or Password").ConfigureAwait(false);
                    return;
                }

                Logger.Info("Session " + session.Id + " authenticated account '" + accountName + "', sending response: " + rspauth);
                await session.ChallengeAsync("rspauth=" + rspauth, account).ConfigureAwait(false);
            }
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            AuthSession authSession = (AuthSession)session;

            if(authSession.Authenticated) {
                await CompleteAuthenticationAsync(authSession).ConfigureAwait(false);
                return;
            }

            if(!authSession.Authenticating) {
                await authSession.FailureAsync("Not Authenticating").ConfigureAwait(false);
                return;
            }

            if(authSession.AuthNonce.Expired) {
                await authSession.FailureAsync("Session Expired").ConfigureAwait(false);
                return;
            }

            ResponseMessage responseMessage = (ResponseMessage)message;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(responseMessage.Response));
            Logger.Debug("Decoded response: " + decoded);

            Dictionary<string, string> values = AuthUtil.ParseDigestValues(decoded);
            if(null == values || 0 == values.Count) {
                await authSession.FailureAsync("Invalid Response").ConfigureAwait(false);
                return;
            }

            try {
                string username = values["username"].Trim(new char[] { '"' });
                await EventLogger.Instance.BeginEventAsync(authSession.RemoteEndPoint, username).ConfigureAwait(false);

                string charset = values["charset"].Trim(new char[] { '"' });
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.FailureAsync("Invalid Response").ConfigureAwait(false);
                    return;
                }

                string qop = values["qop"].Trim(new char[] { '"' });
                if(!"auth".Equals(qop, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.FailureAsync("Invalid Response").ConfigureAwait(false);
                    return;
                }

                string realm = values["realm"].Trim(new char[] { '"' });
                if(!ConfigurationManager.AppSettings["authRealm"].Equals(realm, StringComparison.InvariantCultureIgnoreCase)) {
                    await authSession.FailureAsync("Invalid Response").ConfigureAwait(false);
                    return;
                }

                string nonce = values["nonce"].Trim(new char[] { '"' });
                if(!authSession.AuthNonce.NonceHash.Equals(nonce)) {
                    await authSession.FailureAsync("Invalid Response").ConfigureAwait(false);
                    return;
                }

                string digestURI = values["digest-uri"].Trim(new char[] { '"' });
                //// TODO: validate the digest-uri

                string cnonce = values["cnonce"].Trim(new char[] { '"' });
                string nc = values["nc"].Trim(new char[] { '"' });
                string rsp = values["response"].Trim(new char[] { '"' });

                await AuthenticateAsync(authSession, username, nonce, cnonce, nc, qop, digestURI, rsp).ConfigureAwait(false);
            } catch(KeyNotFoundException) {
                authSession.FailureAsync("Invalid response!").Wait();
            }
        }
    }
}
