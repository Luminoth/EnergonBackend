using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database;
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

        private object _lock = new object();

        public int Id { get; private set; }

#region Network Properties
        private SocketState _socketState;
        public Socket Socket { get { return _socketState.Socket; } }
        private BufferedSocketReader Reader { get { return _socketState.Reader; } }

        private IMessageFormatter _formatter = new BinaryMessageFormatter();

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
        public AuthType AuthType { get; set; }
        public Nonce AuthNonce { get; set; }

        public bool Authenticating { get; set; }
        public bool Authenticated { get; set; }
#endregion

#region Account Properties
        public AccountInfo AccountInfo { get; private set; }
#endregion

#region Message properties
        private ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();
        private MessageHandler _currentMessageHandler;
#endregion

        public Session(Socket socket)
        {
            Id = NextId;
            AuthType = AuthType.None;

            _socketState = new SocketState(socket);
        }

        public void Run()
        {
            lock(_lock) {
                try {
                    // cleanup the current message handler
                    if(null != _currentMessageHandler && _currentMessageHandler.Finished) {
                        _currentMessageHandler = null;
                    }

                    // read off the data buffer and queue any complete messages
                    NetworkMessage message = NetworkMessage.Parse(Reader.Buffer, _formatter);
                    while(null != message) {
                        _logger.Debug("Session " + Id + " parsed message type: " + message.Payload.Type);
                        _messages.Enqueue(message.Payload);
                        message = NetworkMessage.Parse(Reader.Buffer, _formatter);
                    }

                    // handle the next message if we can
                    IMessage nextMessage;
                    if(null == _currentMessageHandler && _messages.TryDequeue(out nextMessage)) {
                        _currentMessageHandler = MessageHandler.Create(nextMessage.Type);
                        if(null != _currentMessageHandler) {
                            ThreadPool.QueueUserWorkItem(_currentMessageHandler.HandleMessage, new MessageHandlerContext(this, nextMessage));
                        }
                    }

                    // TODO: we need a way to say "hey, this handler is taking WAY too long,
                    // dump an error and kill the session"
                } catch(Exception e) {
                    Error("Unhandled Exception!", e);
                }
            }
        }

        public void Error(string error)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error);
            Disconnect();
        }

        public void Error(string error, Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error, ex);
            Disconnect();
        }

        public void Error(Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error", ex);
            Disconnect();
        }

#region Network Methods
        public void Disconnect()
        {
            lock(_lock) {
                if(Socket.Connected) {
                    _logger.Info("Session " + Id + " disconnecting...");
                    _socketState.ShutdownAndClose(true);
                }

                Authenticated = false;
            }
        }

        public void Poll()
        {
            if(!Socket.Connected) {
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
            if(!Socket.Connected) {
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
            lock(_lock) {
                Authenticating = true;
                Authenticated = false;
            }

            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);
        }

        public void Challenge(string challenge, AccountInfo accountInfo)
        {
            InstanceNotifier.Instance.Authenticating(accountInfo.Username, Socket.RemoteEndPoint);

            lock(_lock) {
                AccountInfo = accountInfo;
                Authenticated = true;
            }

            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);
        }

        public void Success(string sessionid)
        {
            InstanceNotifier.Instance.Authenticated(AccountInfo.Username, sessionid, Socket.RemoteEndPoint);
            EventLogger.Instance.SuccessEvent(Socket.RemoteEndPoint, AccountInfo.Username);

            lock(_lock) {
                Authenticating = false;
                Authenticated = true;

                AccountInfo.SessionId = sessionid;
                AccountInfo.SessionEndPoint = Socket.RemoteEndPoint.ToString();

                using(DatabaseConnection connection = ServerState.Instance.AcquireDatabaseConnection()) {
                    AccountInfo.Update(connection);
                }
            }

            SuccessMessage message = new SuccessMessage();
            message.SessionId = sessionid;
            SendMessage(message);

            Disconnect();
        }

        public void Failure(string reason)
        {
            EventLogger.Instance.FailedEvent(Socket.RemoteEndPoint, null == AccountInfo ? null : AccountInfo.Username, reason);

            lock(_lock) {
                Authenticating = false;
                Authenticated = false;

                // NOTE: we don't update the database here because this
                // might be a malicious login attempt against an account
                // that is already using a legitimate ticket

                AccountInfo = null;
            }

            FailureMessage message = new FailureMessage();
            message.Reason = reason;
            SendMessage(message);

            Disconnect();
        }
    }
}
