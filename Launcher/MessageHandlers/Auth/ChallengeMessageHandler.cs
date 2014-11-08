using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using log4net;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    sealed class ChallengeMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChallengeMessageHandler));

        AuthSession _session;

        internal ChallengeMessageHandler(AuthSession session)
        {
            _session = session;
        }

        private void HandleChallengeState(string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                ClientState.Instance.Error("Invalid challenge!");
                return;
            }

            try {
                string charset = values["charset"].Trim(new char[]{'"'});
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    ClientState.Instance.Error("Invalid charset!");
                    return;
                }

                string realm = values["realm"].Trim(new char[]{'"'});
                Nonce cnonce = new Nonce(realm, -1);
                string nc = "00000001";
                string digestURI = realm + "/" + ConfigurationManager.AppSettings["authHost"];

                _logger.Debug("Authenticating " + AuthManager.Instance.Username + ":" + realm + ":***");
                string passwordHash = new SHA512().DigestPassword(
                    AuthManager.Instance.Username,
                    realm,
                    AuthManager.Instance.Password
                );
                _logger.Debug("passwordHash=" + passwordHash);
            
                string nonce = values["nonce"].Trim(new char[]{'"'});
                string qop = values["qop"].Trim(new char[]{'"'});
                string rsp = EnergonSoftware.Core.Auth.DigestClientResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI);
            
                string msg = "username=\"" + AuthManager.Instance.Username + "\","
                    + "realm=" + realm + ","
                        + "nonce=" + nonce + ","
                        + "cnonce=\"" + cnonce.NonceHash + "\","
                        + "nc=" + nc + ","
                        + "qop=" + qop + ","
                        + "digest-uri=\"" + digestURI + "\","
                        + "response=" + rsp + ","
                        + "charset=" + charset;
                _logger.Debug("Generated response: " + msg);

                AuthManager.Instance.AuthResponse(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)),
                    EnergonSoftware.Core.Auth.DigestServerResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI));
            } catch(KeyNotFoundException e) {
                ClientState.Instance.Error("Invalid challenge: " + e.Message);
            }
        }

        private void HandleResponseState(string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                ClientState.Instance.Error("Invalid challenge!");
                return;
            }

            try {
                string rspauth = values["rspauth"].Trim(new char[]{'"'});
                if(AuthManager.Instance.RspAuth != rspauth) {
                    ClientState.Instance.Error("rspauth mismatch, expected: '" + AuthManager.Instance.RspAuth + "', got: '" + rspauth + "'");
                    return;
                }

                AuthManager.Instance.AuthFinalize();
            } catch(KeyNotFoundException e) {
                ClientState.Instance.Error("Invalid challenge: " + e.Message);
                return;
            }
        }

        protected override void OnHandleMessage(IMessage message)
        {
            ChallengeMessage challenge = (ChallengeMessage)message;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challenge.Challenge));
            _logger.Debug("Decoded challenge: " + decoded);

            switch(AuthManager.Instance.AuthStage)
            {
            case AuthManager.AuthenticationStage.Begin:
                HandleChallengeState(decoded);
                break;
            case AuthManager.AuthenticationStage.Challenge:
                HandleResponseState(decoded);
                break;
            default:
                ClientState.Instance.Error("Unexpected auth stage: " + AuthManager.Instance.AuthStage);
                return;
            }
        }
    }
}
