using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;

using EnergonSoftware.Launcher.Net;

using log4net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class ChallengeMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChallengeMessageHandler));

        internal ChallengeMessageHandler()
        {
        }

        private async Task HandleChallengeStateAsync(AuthSession session, string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                await session.ErrorAsync("Invalid challenge!").ConfigureAwait(false);
                return;
            }

            try {
                string charset = values["charset"].Trim(new char[] { '"' });
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    await session.ErrorAsync("Invalid charset!").ConfigureAwait(false);
                    return;
                }

                string realm = values["realm"].Trim(new char[] { '"' });
                Nonce cnonce = new Nonce(realm, -1);
                string nc = "00000001";
                string digestURI = realm + "/" + ConfigurationManager.AppSettings["authHost"];

                Logger.Debug("Authenticating " + App.Instance.UserAccount.AccountName + ":" + realm + ":***");
                string passwordHash = await new SHA512().DigestPasswordAsync(
                    App.Instance.UserAccount.AccountName,
                    realm,
                    App.Instance.UserAccount.Password).ConfigureAwait(false);
                Logger.Debug("passwordHash=" + passwordHash);

                string nonce = values["nonce"].Trim(new char[] { '"' });
                string qop = values["qop"].Trim(new char[] { '"' });
                string rsp = await EnergonSoftware.Core.Auth.DigestClientResponseAsync(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI).ConfigureAwait(false);
            
                string msg = "username=\"" + App.Instance.UserAccount.AccountName + "\","
                    + "realm=" + realm + ","
                        + "nonce=" + nonce + ","
                        + "cnonce=\"" + cnonce.NonceHash + "\","
                        + "nc=" + nc + ","
                        + "qop=" + qop + ","
                        + "digest-uri=\"" + digestURI + "\","
                        + "response=" + rsp + ","
                        + "charset=" + charset;
                Logger.Debug("Generated response: " + msg);

                string rspAuth = await EnergonSoftware.Core.Auth.DigestServerResponseAsync(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI).ConfigureAwait(false);
                await session.AuthResponseAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)), rspAuth).ConfigureAwait(false);
            } catch(KeyNotFoundException e) {
                session.ErrorAsync("Invalid challenge: " + e.Message).Wait();
            }
        }

        private async Task HandleResponseStateAsync(AuthSession session, string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                await session.ErrorAsync("Invalid challenge!").ConfigureAwait(false);
                return;
            }

            try {
                string rspauth = values["rspauth"].Trim(new char[] { '"' });
                if(session.RspAuth != rspauth) {
                    await session.ErrorAsync("rspauth mismatch, expected: '" + session.RspAuth + "', got: '" + rspauth + "'").ConfigureAwait(false);
                    return;
                }

                await session.AuthFinalizeAsync().ConfigureAwait(false);
            } catch(KeyNotFoundException e) {
                session.ErrorAsync("Invalid challenge: " + e.Message).Wait();
                return;
            }
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            ChallengeMessage challengeMessage = (ChallengeMessage)message;
            AuthSession authSession = (AuthSession)session;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challengeMessage.Challenge));
            Logger.Debug("Decoded challenge: " + decoded);

            switch(App.Instance.AuthStage)
            {
            case AuthenticationStage.Begin:
                await HandleChallengeStateAsync(authSession, decoded).ConfigureAwait(false);
                break;
            case AuthenticationStage.Challenge:
                await HandleResponseStateAsync(authSession, decoded).ConfigureAwait(false);
                break;
            default:
                await authSession.ErrorAsync("Unexpected auth stage: " + App.Instance.AuthStage).ConfigureAwait(false);
                return;
            }
        }
    }
}
