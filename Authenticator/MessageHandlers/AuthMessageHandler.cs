using System;
using System.Configuration;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Net;

using EnergonSoftware.Backend;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class AuthMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthMessageHandler));

        [Obsolete]
        private static string BuildDigestMD5Challenge(Nonce nonce)
        {
            Logger.Debug("Building MD5 challenge...");

            return "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + nonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=md5-sess";
        }

        private static string BuildDigestSHA512Challenge(Nonce nonce)
        {
            Logger.Debug("Building SHA512 challenge...");

            return "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + nonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=sha512-sess";
        }

        internal AuthMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            AuthSession authSession = (AuthSession)session;

            await EventLogger.Instance.RequestEventAsync(authSession.RemoteEndPoint).ConfigureAwait(false);

            if(authSession.Authenticated || authSession.Authenticating) {
                await authSession.FailureAsync("Already Authenticating").ConfigureAwait(false);
                return;
            }

            AuthMessage authMessage = (AuthMessage)message;
            if(Common.AuthVersion != authMessage.Version) {
                Logger.Debug("Bad version, expected: " + Common.AuthVersion + ", got: " + authMessage.Version);
                await authSession.FailureAsync("Bad Version").ConfigureAwait(false);
                return;
            }

            Nonce nonce = new Nonce(ConfigurationManager.AppSettings["authRealm"], Convert.ToInt32(ConfigurationManager.AppSettings["authExpiry"]));

            string challenge = string.Empty;
            switch(authMessage.MechanismType)
            {
            case AuthType.DigestMD5:
                /*challenge = BuildDigestMD5Challenge(nonce);
                break;*/
await authSession.FailureAsync("MD5 mechanism not supported!").ConfigureAwait(false);
return;
            case AuthType.DigestSHA512:
                challenge = BuildDigestSHA512Challenge(nonce);
                break;
            default:
                await authSession.FailureAsync("Unsupported mechanism").ConfigureAwait(false);
                return;
            }

            Logger.Debug("Session " + authSession.Id + " generated challenge: " + challenge);
            await authSession.ChallengeAsync(authMessage.MechanismType, nonce, challenge).ConfigureAwait(false);
        }
    }
}
