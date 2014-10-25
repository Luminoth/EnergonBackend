using System;
using System.Collections.Generic;
using System.Text;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class ChallengeMessageHandler : IMessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChallengeMessageHandler));

        internal ChallengeMessageHandler()
        {
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
                string digestURI = realm + "/" + ClientState.Instance.Host;

                _logger.Debug("Authenticating " + ClientState.Instance.Username + ":" + realm + ":***");
                string passwordHash = new SHA512().DigestPassword(
                    ClientState.Instance.Username,
                    realm,
                    ClientState.Instance.Password
                );
                _logger.Debug("passwordHash=" + passwordHash);
            
                string nonce = values["nonce"].Trim(new char[]{'"'});
                string qop = values["qop"].Trim(new char[]{'"'});
                string rsp = EnergonSoftware.Core.Auth.DigestClientResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI);
                ClientState.Instance.RspAuth = EnergonSoftware.Core.Auth.DigestServerResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI);
            
                string msg = "username=\"" + ClientState.Instance.Username + "\","
                    + "realm=" + realm + ","
                        + "nonce=" + nonce + ","
                        + "cnonce=\"" + cnonce.NonceHash + "\","
                        + "nc=" + nc + ","
                        + "qop=" + qop + ","
                        + "digest-uri=\"" + digestURI + "\","
                        + "response=" + rsp + ","
                        + "charset=" + charset;
                _logger.Debug("Generated response: " + msg);

                ClientState.Instance.AuthResponse(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)));
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
                if(ClientState.Instance.RspAuth != rspauth) {
                    ClientState.Instance.Error("rspauth mismatch, expected: '" + ClientState.Instance.RspAuth + "', got: '" + rspauth + "'");
                    return;
                }

                ClientState.Instance.AuthFinalize();
            } catch(KeyNotFoundException e) {
                ClientState.Instance.Error("Invalid challenge: " + e.Message);
                return;
            }
        }

        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            ChallengeMessage challenge = (ChallengeMessage)ctx.Message;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challenge.Challenge));
            _logger.Debug("Decoded challenge: " + decoded);

            switch(ClientState.Instance.AuthStage)
            {
            case AuthenticationStage.Begin:
                HandleChallengeState(decoded);
                break;
            case AuthenticationStage.Challenge:
                HandleResponseState(decoded);
                break;
            default:
                ClientState.Instance.Error("Unexpected auth stage: " + ClientState.Instance.AuthStage);
                return;
            }
        }
    }
}
