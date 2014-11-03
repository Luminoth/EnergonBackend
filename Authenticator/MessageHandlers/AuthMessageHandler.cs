using System;
using System.Configuration;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    sealed class AuthMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthMessageHandler));

        internal AuthMessageHandler()
        {
        }

        /*[Obsolete]
        private void HandleDigestMD5Message(AuthMessage message, Session session)
        {
            _logger.Info("Handling MD5 auth request...");
            session.AuthNonce = new Nonce(ConfigurationManager.AppSettings["authRealm"], Int32.Parse(ConfigurationManager.AppSettings["authExpiry"]));

            string msg = "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + session.AuthNonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=md5-sess";
            _logger.Debug("Session " + session.Id + " generated challenge: " + msg);

            session.AuthChallenge(msg);
        }*/

        private void HandleDigestSHA512Message(AuthMessage message, Session session)
        {
            _logger.Info("Handling SHA512 auth request...");
            session.AuthNonce = new Nonce(ConfigurationManager.AppSettings["authRealm"], Int32.Parse(ConfigurationManager.AppSettings["authExpiry"]));

            string challenge = "realm=\"" + ConfigurationManager.AppSettings["authRealm"] + "\""
                + ",nonce=\"" + session.AuthNonce.NonceHash + "\""
                + ",qop=\"auth\",charset=utf-8,algorithm=sha512-sess";
            _logger.Debug("Session " + session.Id + " generated challenge: " + challenge);

            session.Challenge(challenge);
        }

        protected override void OnHandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;

            EventLogger.Instance.RequestEvent(ctx.Session.Socket.RemoteEndPoint);
            if(ctx.Session.Authenticated || ctx.Session.Authenticating) {
                ctx.Session.Failure("Already Authenticating");
                return;
            }

            AuthMessage message = (AuthMessage)ctx.Message;
            if(Common.AUTH_VERSION != message.Version) {
                ctx.Session.Failure("Bad Version");
                return;
            }

            ctx.Session.AuthType = message.MechanismType;
            switch(ctx.Session.AuthType)
            {
            /*case AuthType.DigestMD5:
                HandleDigestMD5Message(message, session);
                break;*/
            case AuthType.DigestSHA512:
                HandleDigestSHA512Message(message, ctx.Session);
                break;
            default:
                ctx.Session.Failure("Unsupported Mechanism");
                break;
            }
        }
    }
}
