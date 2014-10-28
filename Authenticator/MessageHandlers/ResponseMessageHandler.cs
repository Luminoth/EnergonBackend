using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    sealed class ResponseMessageHandler : IMessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ResponseMessageHandler));

        internal ResponseMessageHandler()
        {
        }

// TODO: handle the case of a user authenticating a second time

        private void CompleteAuthentication(Session session)
        {
            SessionId sessionid = new SessionId(ConfigurationManager.AppSettings["sessionSecret"]);
            _logger.Info("Session " + session.Id + " generated sessionid '" + sessionid.SessionID + "' for account '" + session.AccountInfo.Username + "'");
            session.Success(sessionid.SessionID);

            InstanceNotifier.Instance.Authenticated(session.AccountInfo.Username, sessionid, session.Socket.RemoteEndPoint);
        }

        private void Authenticate(string username, string nonce, string cnonce, string nc, string qop, string digestURI, string response, Session session)
        {
            InstanceNotifier.Instance.Authenticating(username, session.Socket.RemoteEndPoint);

            AccountInfo account = new AccountInfo(username);
            using(DatabaseConnection connection = ServerState.Instance.AcquireDatabaseConnection()) {
                if(!account.Read(connection)) {
                    session.Failure("Bad Username or Password");
                    return;
                }
            }

            if(!account.Active) {
                session.Failure("Account Inactive");
                return;
            }

            string expected = "", rspauth = "";
            switch(session.AuthType)
            {
            /*case Heroes.Core.AuthType.DigestMD5:
                _logger.Debug("Handling MD5 response...");
                //_logger.Debug("passwordHash=" + account.PasswordMD5);
                expected = EnergonSoftware.Core.Auth.DigestClientResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI);
                rspauth = EnergonSoftware.Core.Auth.DigestServerResponse(new MD5(), account.PasswordMD5, nonce, nc, qop, cnonce, digestURI);
                break;*/
            case EnergonSoftware.Core.AuthType.DigestSHA512:
                _logger.Debug("Handling SHA512 response...");
                //_logger.Debug("passwordHash=" + account.PasswordSHA512);
                expected = EnergonSoftware.Core.Auth.DigestClientResponse(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI);
                rspauth = EnergonSoftware.Core.Auth.DigestServerResponse(new SHA512(), account.PasswordSHA512, nonce, nc, qop, cnonce, digestURI);
                break;
            default:
                session.Failure("Unsupported Auth Type");
                return;
            }

            if(!expected.Equals(response)) {
                session.Failure("Bad Username or Password");
                return;
            }

            string challenge = "rspauth=" + rspauth;
            _logger.Info("Session " + session.Id + " authenticated account '" + username + "', sending response: " + rspauth);
            session.Challenge(challenge, account);
        }

        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            ResponseMessage response = (ResponseMessage)ctx.Message;

            if(ctx.Session.Authenticated) {
                CompleteAuthentication(ctx.Session);
                return;
            }

            if(!ctx.Session.Authenticating) {
                ctx.Session.Failure("Not Authenticating");
                return;
            }

            if(ctx.Session.AuthNonce.Expired) {
                ctx.Session.Failure("Session Expired");
                return;
            }

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response.Response));
            _logger.Debug("Decoded response: " + decoded);

            Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(decoded);
            if(null == values || 0 == values.Count) {
                ctx.Session.Failure("Invalid Response");
                return;
            }

            try {
                string username = values["username"].Trim(new char[]{'"'});
                EventLogger.Instance.BeginEvent(ctx.Session.Socket.RemoteEndPoint, username);

                string charset = values["charset"].Trim(new char[]{'"'});
                if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                    ctx.Session.Failure("Invalid Response");
                    return;
                }

                string qop = values["qop"].Trim(new char[]{'"'});
                if(!"auth".Equals(qop, StringComparison.InvariantCultureIgnoreCase)) {
                    ctx.Session.Failure("Invalid Response");
                    return;
                }

                string realm = values["realm"].Trim(new char[]{'"'});
                if(!ConfigurationManager.AppSettings["authRealm"].Equals(realm, StringComparison.InvariantCultureIgnoreCase)) {
                    ctx.Session.Failure("Invalid Response");
                    return;
                }

                string nonce = values["nonce"].Trim(new char[]{'"'});
                if(!ctx.Session.AuthNonce.NonceHash.Equals(nonce)) {
                    ctx.Session.Failure("Invalid Response");
                    return;
                }

                string digestURI = values["digest-uri"].Trim(new char[]{'"'});
                // TODO: validate the digest-uri

                string cnonce = values["cnonce"].Trim(new char[]{'"'});
                string nc = values["nc"].Trim(new char[]{'"'});
                string rsp = values["response"].Trim(new char[]{'"'});

                Authenticate(username, nonce, cnonce, nc, qop, digestURI, rsp, ctx.Session);
            } catch(KeyNotFoundException) {
                ctx.Session.Failure("Invalid response!");
            }
        }
    }
}
