using System;
using System.Configuration;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;
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

        protected async override void OnHandleMessage(IMessage message, Session session)
        {
            AuthSession authSession = (AuthSession)session;

            await EventLogger.Instance.RequestEvent(authSession.RemoteEndPoint);
            if(authSession.Authenticated || authSession.Authenticating) {
                await authSession.Failure("Already Authenticating");
                return;
            }

            AuthMessage authMessage = (AuthMessage)message;
            if(Common.AuthVersion != authMessage.Version) {
                await authSession.Failure("Bad Version");
                return;
            }

            Nonce nonce = new Nonce(ConfigurationManager.AppSettings["authRealm"], Convert.ToInt32(ConfigurationManager.AppSettings["authExpiry"]));

            string challenge = string.Empty;
            switch(authMessage.MechanismType)
            {
            case AuthType.DigestMD5:
                /*challenge = BuildDigestMD5Challenge(nonce);
                break;*/
await authSession.Failure("MD5 mechanism not supported!");
return;
            case AuthType.DigestSHA512:
                challenge = BuildDigestSHA512Challenge(nonce);
                break;
            default:
                await authSession.Failure("Unsupported mechanism");
                return;
            }

            Logger.Debug("Session " + authSession.Id + " generated challenge: " + challenge);
            authSession.Challenge(authMessage.MechanismType, nonce, challenge);
        }
    }
}
