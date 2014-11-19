﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Core.Util.Crypt;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    sealed class ResponseMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ResponseMessageHandler));

        private readonly AuthSession _session;

        internal ResponseMessageHandler(AuthSession session)
        {
            _session = session;
        }

// TODO: handle the case of a user authenticating a second time

        private void CompleteAuthentication()
        {
            SessionId sessionid = new SessionId(ConfigurationManager.AppSettings["sessionSecret"]);
            _logger.Info("Session " + _session.Id + " generated sessionid '" + sessionid.SessionID + "' for account '" + _session.AccountInfo.Username + "'");
            _session.Success(sessionid.SessionID);
        }

        private void Authenticate(string username, string nonce, string cnonce, string nc, string qop, string digestURI, string response)
        {
            AccountInfo account = new AccountInfo(username);
            using(DatabaseConnection connection = DatabaseManager.AcquireDatabaseConnection()) {
                if(!account.Read(connection)) {
                    _session.Failure("Bad Username or Password");
                    return;
                }
            }

            if(!account.Active) {
                _session.Failure("Account Inactive");
                return;
            }

            string expected = "", rspauth = "";
            switch(_session.AuthType)
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
                _session.Failure("Unsupported Auth Type");
                return;
            }

            if(!expected.Equals(response)) {
                _session.Failure("Bad Username or Password");
                return;
            }

            string challenge = "rspauth=" + rspauth;
            _logger.Info("Session " + _session.Id + " authenticated account '" + username + "', sending response: " + rspauth);
            _session.Challenge(challenge, account);
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    if(_session.Authenticated) {
                        CompleteAuthentication();
                        return;
                    }

                    if(!_session.Authenticating) {
                        _session.Failure("Not Authenticating");
                        return;
                    }

                    if(_session.AuthNonce.Expired) {
                        _session.Failure("Session Expired");
                        return;
                    }

                    ResponseMessage response = (ResponseMessage)message;

                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response.Response));
                    _logger.Debug("Decoded response: " + decoded);

                    Dictionary<string, string> values = EnergonSoftware.Core.Auth.ParseDigestValues(decoded);
                    if(null == values || 0 == values.Count) {
                        _session.Failure("Invalid Response");
                        return;
                    }

                    try {
                        string username = values["username"].Trim(new char[]{'"'});
                        EventLogger.Instance.BeginEvent(_session.RemoteEndPoint, username);

                        string charset = values["charset"].Trim(new char[]{'"'});
                        if(!"utf-8".Equals(charset, StringComparison.InvariantCultureIgnoreCase)) {
                            _session.Failure("Invalid Response");
                            return;
                        }

                        string qop = values["qop"].Trim(new char[]{'"'});
                        if(!"auth".Equals(qop, StringComparison.InvariantCultureIgnoreCase)) {
                            _session.Failure("Invalid Response");
                            return;
                        }

                        string realm = values["realm"].Trim(new char[]{'"'});
                        if(!ConfigurationManager.AppSettings["authRealm"].Equals(realm, StringComparison.InvariantCultureIgnoreCase)) {
                            _session.Failure("Invalid Response");
                            return;
                        }

                        string nonce = values["nonce"].Trim(new char[]{'"'});
                        if(!_session.AuthNonce.NonceHash.Equals(nonce)) {
                            _session.Failure("Invalid Response");
                            return;
                        }

                        string digestURI = values["digest-uri"].Trim(new char[]{'"'});
                        // TODO: validate the digest-uri

                        string cnonce = values["cnonce"].Trim(new char[]{'"'});
                        string nc = values["nc"].Trim(new char[]{'"'});
                        string rsp = values["response"].Trim(new char[]{'"'});

                        Authenticate(username, nonce, cnonce, nc, qop, digestURI, rsp);
                    } catch(KeyNotFoundException) {
                        _session.Failure("Invalid response!");
                    }
                }
            );
        }
    }
}
