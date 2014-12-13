﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;
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

        private void HandleChallengeState(AuthSession session, string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                session.Error("Invalid challenge!");
                return;
            }

            try {
                string charset = values["charset"].Trim(new char[] { '"' });
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    session.Error("Invalid charset!");
                    return;
                }

                string realm = values["realm"].Trim(new char[] { '"' });
                Nonce cnonce = new Nonce(realm, -1);
                string nc = "00000001";
                string digestURI = realm + "/" + ConfigurationManager.AppSettings["authHost"];

                Logger.Debug("Authenticating " + ClientState.Instance.Username + ":" + realm + ":***");
                string passwordHash = new SHA512().DigestPassword(
                    ClientState.Instance.Username,
                    realm,
                    ClientState.Instance.Password
                );
                Logger.Debug("passwordHash=" + passwordHash);
            
                string nonce = values["nonce"].Trim(new char[] { '"' });
                string qop = values["qop"].Trim(new char[] { '"' });
                string rsp = EnergonSoftware.Core.Auth.DigestClientResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI);
            
                string msg = "username=\"" + ClientState.Instance.Username + "\","
                    + "realm=" + realm + ","
                        + "nonce=" + nonce + ","
                        + "cnonce=\"" + cnonce.NonceHash + "\","
                        + "nc=" + nc + ","
                        + "qop=" + qop + ","
                        + "digest-uri=\"" + digestURI + "\","
                        + "response=" + rsp + ","
                        + "charset=" + charset;
                Logger.Debug("Generated response: " + msg);

                session.AuthResponse(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)),
                    EnergonSoftware.Core.Auth.DigestServerResponse(new SHA512(), passwordHash, nonce, nc, qop, cnonce.NonceHash, digestURI));
            } catch(KeyNotFoundException e) {
                session.Error("Invalid challenge: " + e.Message);
            }
        }

        private void HandleResponseState(AuthSession session, string message)
        {
            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(message);
            if(null == values || 0 == values.Count) {
                session.Error("Invalid challenge!");
                return;
            }

            try {
                string rspauth = values["rspauth"].Trim(new char[] { '"' });
                if(session.RspAuth != rspauth) {
                    session.Error("rspauth mismatch, expected: '" + session.RspAuth + "', got: '" + rspauth + "'");
                    return;
                }

                session.AuthFinalize();
            } catch(KeyNotFoundException e) {
                session.Error("Invalid challenge: " + e.Message);
                return;
            }
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            ChallengeMessage challenge = (ChallengeMessage)message;
            AuthSession authSession = (AuthSession)session;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challenge.Challenge));
            Logger.Debug("Decoded challenge: " + decoded);

            switch(authSession.AuthStage)
            {
            case AuthenticationStage.Begin:
                HandleChallengeState(authSession, decoded);
                break;
            case AuthenticationStage.Challenge:
                HandleResponseState(authSession, decoded);
                break;
            default:
                authSession.Error("Unexpected auth stage: " + authSession.AuthStage);
                return;
            }
        }
    }
}
