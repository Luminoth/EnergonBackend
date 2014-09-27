using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Database.Objects.Events;
using EnergonSoftware.Authenticator.MessageHandlers;

namespace EnergonSoftware.Authenticator.Net
{
    sealed class Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Session));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        private int _id = -1;
        public int Id { get { return _id; } }

        private object _lock = new object();

#region Network Properties
        private volatile SocketState _socketState;
        private volatile IMessageFormatter _formatter = new BinaryMessageFormatter();

        private Socket Socket { get { return _socketState.Socket; } }
        private BufferedSocketReader Reader { get { return _socketState.Reader; } }

        public EndPoint RemoteEndpoint { get { return Socket.RemoteEndPoint; } }
        public bool Connected { get { return _socketState.Connected; } }

        public bool TimedOut
        {
            get
            {
                int timeout = Int32.Parse(ConfigurationManager.AppSettings["sessionTimeout"]);
                if(timeout < 0) {
                    return false;
                }
                return Time.CurrentTimeMs >= (Reader.LastMessageTime + timeout);
            }
        }
#endregion

#region Auth Properties
        private volatile AuthType _authType = AuthType.None;
        private volatile bool _authenticating = false;
        private volatile bool _authenticated = false;

        public AuthType AuthType { get { return _authType; } set { _authType = value; } }
        public Nonce AuthNonce { get; set; }

        public bool Authenticating { get { return Connected && _authenticating; } }
        public bool Authenticated { get { return Connected && _authenticated; } }
#endregion

        private volatile AccountInfo _accountInfo;
        public AccountInfo AccountInfo { get { return _accountInfo; } }

        public Session(Socket socket)
        {
            _id = NextId;
            _socketState = new SocketState(socket);
        }

        public void Run()
        {
            lock(_lock) {
                try {
                    NetworkMessage message = NetworkMessage.Parse(Reader.Buffer, _formatter);
                    if(null != message) {
                        _logger.Debug("Session " + Id + " parsed message type: " + message.Payload.Type);
                        MessageHandler.HandleMessage(message.Payload, this);
                    }
                } catch(Exception e) {
                    Error(e);
                }
            }
        }

        public void Error(string error)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error);
            Disconnect();
        }

        public void Error(Exception error)
        {
            Error(error.Message);
        }

#region Network Methods
        public void Disconnect()
        {
            lock(_lock) {
                if(Connected) {
                    _logger.Info("Session " + Id + " disconnecting...");
                    _socketState.ShutdownAndClose(true);
                }

                _authenticated = false;
            }
        }

        public void Poll()
        {
            if(!Connected) {
                return;
            }

            lock(_lock) {
                try {
                    if(Socket.Poll(100, SelectMode.SelectRead)) {
                        int len = Reader.Read();
                        if(0 == len) {
                            Error("End of stream!");
                            return;
                        }
                        _logger.Debug("Session " + Id + " read " + len + " bytes");
                    }
                } catch(SocketException e) {
                    Error(e);
                }
            }
        }

        public void SendMessage(IMessage message)
        {
            if(!Connected) {
                return;
            }

            NetworkMessage packet = new NetworkMessage();
            packet.Payload = message;

            byte[] bytes = packet.Serialize(_formatter);
            lock(_lock) {
                _logger.Debug("Session " + Id + " sending " + bytes.Length + " bytes");
                Socket.Send(bytes);
            }
        }
#endregion

        public void Challenge(string challenge)
        {
            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);

            lock(_lock) {
                _authenticating = true;
                _authenticated = false;
            }
        }

        public void Challenge(string challenge, AccountInfo accountInfo)
        {
            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);

            lock(_lock) {
                _accountInfo = accountInfo;
                _authenticated = true;
            }
        }

        public void Success(string sessionid)
        {
            EventLogger.Instance.SuccessEvent(RemoteEndpoint.ToString(), AccountInfo.Username);

            SuccessMessage message = new SuccessMessage();
            message.SessionId = sessionid;
            SendMessage(message);

            lock(_lock) {
                AccountInfo.SessionId = sessionid;
                _authenticating = false;
            }
        }

        public void Failure(string reason=null)
        {
            EventLogger.Instance.SuccessEvent(RemoteEndpoint.ToString(), null == AccountInfo ? null : AccountInfo.Username);

            FailureMessage message = new FailureMessage();
            message.Reason = reason;
            SendMessage(message);

            lock(_lock) {
                _authenticating = false;
                _authenticating = false;
                _accountInfo = null;
            }
            Disconnect();
        }
    }
}
