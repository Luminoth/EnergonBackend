using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

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

        private async Task HandleChallengeStateAsync(AuthSession session, string message)
        {
            Dictionary<string, string> values = AuthUtil.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                await session.ErrorAsync("Invalid challenge!").ConfigureAwait(false);
                return;
            }

            try {
                string charset = values["charset"].Trim('"');
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    await session.ErrorAsync("Invalid charset!").ConfigureAwait(false);
                    return;
                }

                string realm = values["realm"].Trim('"');
                Nonce cnonce = new Nonce(realm, -1);
                string nc = "00000001";
                string digestUri = $"{realm}/{ConfigurationManager.AppSettings["authHost"]}";

                Logger.Debug($"Authenticating {App.Instance.UserAccount.AccountName}:{realm}:****");
                string passwordHash = await new SHA512().DigestPasswordAsync(
                    App.Instance.UserAccount.AccountName,
                    realm,
                    App.Instance.UserAccount.Password).ConfigureAwait(false);
                Logger.Debug($"passwordHash={passwordHash}");

                string nonce = values["nonce"].Trim('"');
                string qop = values["qop"].Trim('"');
                string rsp = await AuthUtil.DigestClientResponseAsync(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestUri).ConfigureAwait(false);

                string msg = $"username=\"{App.Instance.UserAccount.AccountName}\",realm={realm},"
                    + $"nonce={nonce},cnonce=\"{cnonce.NonceHash}\",nc={nc},"
                    + $"qop={qop},digest-uri=\"{digestUri}\",response={rsp},charset={charset}";
                Logger.Debug($"Generated response: {msg}");

                string rspAuth = await AuthUtil.DigestServerResponseAsync(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestUri).ConfigureAwait(false);
                await session.AuthResponseAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)), rspAuth).ConfigureAwait(false);
            } catch(KeyNotFoundException e) {
                await session.ErrorAsync($"Invalid challenge: {e.Message}").ConfigureAwait(false);
            }
        }

        private async Task HandleResponseStateAsync(AuthSession session, string message)
        {
            Dictionary<string, string> values = AuthUtil.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                await session.ErrorAsync("Invalid challenge!").ConfigureAwait(false);
                return;
            }

            try {
                string rspauth = values["rspauth"].Trim('"');
                if(session.RspAuth != rspauth) {
                    await session.ErrorAsync($"rspauth mismatch, expected: '{session.RspAuth}', got: '{rspauth}'").ConfigureAwait(false);
                    return;
                }

                await session.AuthFinalizeAsync().ConfigureAwait(false);
            } catch(KeyNotFoundException e) {
                await session.ErrorAsync($"Invalid challenge: {e.Message}").ConfigureAwait(false);
            }
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            ChallengeMessage challengeMessage = (ChallengeMessage)message;
            AuthSession authSession = (AuthSession)session;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challengeMessage.Challenge));
            Logger.Debug($"Decoded challenge: {decoded}");

            switch(App.Instance.AuthStage)
            {
            case AuthenticationStage.Begin:
                await HandleChallengeStateAsync(authSession, decoded).ConfigureAwait(false);
                break;
            case AuthenticationStage.Challenge:
                await HandleResponseStateAsync(authSession, decoded).ConfigureAwait(false);
                break;
            default:
                await authSession.ErrorAsync($"Unexpected auth stage: {App.Instance.AuthStage}").ConfigureAwait(false);
                return;
            }
        }
    }
}
