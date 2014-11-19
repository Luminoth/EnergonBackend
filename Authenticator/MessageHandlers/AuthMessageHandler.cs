using System;
using System.Configuration;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    sealed class AuthMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthMessageHandler));

        [Obsolete]
        private static string BuildDigestMD5Challenge(Nonce nonce)
        {
            _logger.Debug("Building MD5 challenge...");

            return "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + nonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=md5-sess";
        }

        private static string BuildDigestSHA512Challenge(Nonce nonce)
        {
            _logger.Debug("Building SHA512 challenge...");

            return "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + nonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=sha512-sess";
        }

        private readonly AuthSession _session;

        internal AuthMessageHandler(AuthSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    EventLogger.Instance.RequestEvent(_session.RemoteEndPoint);
                    if(_session.Authenticated || _session.Authenticating) {
                        _session.Failure("Already Authenticating");
                        return;
                    }

                    AuthMessage auth = (AuthMessage)message;
                    if(Common.AUTH_VERSION != auth.Version) {
                        _session.Failure("Bad Version");
                        return;
                    }

                    Nonce nonce = new Nonce(ConfigurationManager.AppSettings["authRealm"], Convert.ToInt32(ConfigurationManager.AppSettings["authExpiry"]));

                    string challenge;
                    switch(auth.MechanismType)
                    {
                    /*case AuthType.DigestMD5:
                        challenge = BuildDigestMD5Challenge(nonce);
                        break;*/
                    case AuthType.DigestSHA512:
                        challenge = BuildDigestSHA512Challenge(nonce);
                        break;
                    default:
                        _session.Failure("Unsupported Mechanism");
                        return;
                    }

                    _logger.Debug("Session " + _session.Id + " generated challenge: " + challenge);
                    _session.Challenge(auth.MechanismType, nonce, challenge);
                }
            );
        }
    }
}
