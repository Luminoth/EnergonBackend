using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using EnergonSoftware.Launcher.MessageHandlers;

namespace EnergonSoftware.Launcher
{
    internal enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    sealed class ClientState : ClientApi
    {
#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

        private object _lock = new object();

#region Network Properties
        public int AuthSocketId { get; private set; }
        public int OvermindSocketId { get; private set; }
#endregion

#region Authentication Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

#region Message properties
        private Dictionary<int, Queue<IMessage>> _messages = new Dictionary<int,Queue<IMessage>>();
        private Dictionary<int, MessageHandler> _currentMessageHandlers = new Dictionary<int,MessageHandler>();
        private IMessageFormatter _formatter = new BinaryMessageFormatter();

        private bool ShouldPing
        {
            get {
                long lastMessageTime = GetLastMessageTime(OvermindSocketId);
                if(0 == lastMessageTime) {
                    return false;
                }
                return Time.CurrentTimeMs > (lastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"]));
            }
        }
#endregion

#region Authentication Properties
        private AuthenticationStage _authStage = AuthenticationStage.NotAuthenticated;
        internal AuthenticationStage AuthStage
        {
            get { return _authStage; }
            set {
                _authStage = value;
                NotifyPropertyChanged("Authenticating");
                NotifyPropertyChanged("Authenticated");
            }
        }

        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        private string _username;
        public string Username { get { return _username; } set { _username = value; NotifyPropertyChanged("Username"); } }

        private string _password;
        public string Password { get { return _password; } set { _password = value; NotifyPropertyChanged("Password"); } }

        internal string RspAuth { get; set; }
#endregion

#region Login Properties
        private bool _loggedIn = false;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            set {
                _loggedIn = value;
                NotifyPropertyChanged("LoggedIn");
                NotifyPropertyChanged("NotLoggedIn");
            }
        }
#endregion

#region UI Helpers
        private bool _loggingIn = false;
        public bool LoggingIn
        {
            get { return _loggingIn; }
            set {
                _loggingIn = value;
                NotifyPropertyChanged("LoggingIn");
                NotifyPropertyChanged("CanLogin");
            }
        }

        public bool CanLogin { get { return !LoggingIn && !LoggedIn; } }
        public bool NotLoggedIn { get { return !LoggedIn; } }
#endregion

        private void OnSocketErrorCallback(int socketId, string error)
        {
            if(socketId == AuthSocketId) {
                Disconnect(AuthSocketId);
            } else if(socketId == OvermindSocketId) {
                Disconnect(OvermindSocketId);
            }
        }

        private void OnDisconnectCallback(int socketId)
        {
            if(socketId == AuthSocketId) {
                if(!Authenticated) {
                    Error("Auth server disconnected!");
                }
                AuthSocketId = -1;
            } else if(socketId == OvermindSocketId) {
                Error("Overmind disconnected!");
                OvermindSocketId = -1;
                LoggedIn = false;
            } else {
                Error("Server disconnected!");
            }
        }

#region Network Methods
        private void OnConnectFailedCallback(int socketId, SocketError error)
        {
            if(AuthSocketId == socketId) {
                AuthFailed("Failed to connect to the authentication server: " + error);
            } else if(OvermindSocketId == socketId) {
                Error("Failed to connect to the overmind server: " + error);
            }
        }

        private void OnConnectSuccessCallback(int socketId)
        {
            if(AuthSocketId == socketId) {
                BeginAuth();
            } else if(OvermindSocketId == socketId) {
                Login();
            }
        }
#endregion

#region Authentication Methods
        // TODO: this is silly, don't pass the password in here
        // should use a more "proper" way of querying for it
        internal void AuthConnect(string password)
        {
            lock(_lock) {
                Password = password;

                AuthSocketId = ConnectAsync(ConfigurationManager.AppSettings["authHost"], Int32.Parse(ConfigurationManager.AppSettings["authPort"]));
            }
        }

        private void BeginAuth()
        {
            _logger.Info("Authenticating as user '" + Username + "'...");

            lock(_lock) {
                AuthMessage message = new AuthMessage();
                message.MechanismType = AuthType.DigestSHA512;
                SendMessage(AuthSocketId, message, _formatter);

                AuthStage = AuthenticationStage.Begin;
            }
        }

        internal void AuthResponse(string response)
        {
            lock(_lock) {
                ResponseMessage message = new ResponseMessage();
                message.Response = response;
                SendMessage(AuthSocketId, message, _formatter);

                AuthStage = AuthenticationStage.Challenge;
            }
        }

        internal void AuthFinalize()
        {
            lock(_lock) {
                ResponseMessage response = new ResponseMessage();
                SendMessage(AuthSocketId, response, _formatter);

                AuthStage = AuthenticationStage.Finalize;
            }
        }

        internal void AuthSuccess(string ticket)
        {
            _logger.Info("Authentication successful!");
            _logger.Debug("Ticket=" + ticket);

            lock(_lock) {
                Ticket = ticket;

                Password = null;
                RspAuth = null;

                AuthStage = AuthenticationStage.Authenticated;

                if(null != OnAuthSuccess) {
                    OnAuthSuccess();
                }

                Disconnect(AuthSocketId);
                AuthSocketId = -1;
            }
        }

        internal void AuthFailed(string reason)
        {
            _logger.Warn("Authentication failed: " + reason);

            lock(_lock) {
                Reset();

                if(null != OnAuthFailed) {
                    OnAuthFailed(reason);
                }

                Disconnect(AuthSocketId);
                AuthSocketId = -1;
            }
        }
#endregion

#region Overmind Methods
        internal void OvermindConnect()
        {
            lock(_lock) {
                OvermindSocketId = ConnectAsync(ConfigurationManager.AppSettings["overmindHost"], Int32.Parse(ConfigurationManager.AppSettings["overmindPort"]));
            }
        }

        private void Login()
        {
            _logger.Info("Logging in...");

            lock(_lock) {
                LoginMessage message = new LoginMessage();
                message.Username = Username;
                message.Ticket = Ticket;
                SendMessage(OvermindSocketId, message, _formatter);

                LoggingIn = false;
                LoggedIn = true;
            }
        }

        private void Ping()
        {
            if(!ShouldPing) {
                return;
            }

            lock(_lock) {
                PingMessage message = new PingMessage();
                SendMessage(OvermindSocketId, message, _formatter);
            }
        }
#endregion

        private void HandleMessages(int socketId)
        {
            if(socketId < 1) {
                return;
            }

            BufferedSocketReader reader = GetSocketReader(socketId);
            if(null == reader) {
                return;
            }

            if(!_messages.ContainsKey(socketId)) {
                _messages[socketId]  = new Queue<IMessage>();
            }

            lock(_lock) {
                // cleanup the current message handler
                if(_currentMessageHandlers.ContainsKey(socketId) && _currentMessageHandlers[socketId].Finished) {
                    _currentMessageHandlers.Remove(socketId);
                }

                // read off the data buffer and queue any complete messages
                NetworkMessage message = NetworkMessage.Parse(reader.Buffer, _formatter);
                while(null != message) {
                    _logger.Debug("Parsed message type: " + message.Payload.Type);
                    _messages[socketId].Enqueue(message.Payload);
                    message = NetworkMessage.Parse(reader.Buffer, _formatter);
                }

                // handle the next message if we can
                if(!_currentMessageHandlers.ContainsKey(socketId) && _messages[socketId].Count > 0) {
                    IMessage nextMessage = _messages[socketId].Dequeue();

                    MessageHandler handler = MessageHandler.Create(nextMessage.Type);
                    if(null != handler) {
                        _currentMessageHandlers[socketId] = handler;
                        ThreadPool.QueueUserWorkItem(_currentMessageHandlers[socketId].HandleMessage, new MessageHandlerContext(nextMessage));
                    }
                }

                // TODO: we need a way to say "hey, this handler is taking WAY too long,
                // dump an error and kill the session"
                return;
            }
        }

        private void HandleMessages()
        {
            HandleMessages(AuthSocketId);
            HandleMessages(OvermindSocketId);
        }

        private void Poll()
        {
            Poll(AuthSocketId);
            Poll(OvermindSocketId);
        }

        public void Run()
        {
            Poll();

            HandleMessages();

            Ping();
        }

        private void Reset()
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
            Ticket = null;
            Password = null;
            RspAuth = null;

            LoggingIn = false;
            LoggedIn = false;
        }

        private ClientState()
        {
            OnSocketError += OnSocketErrorCallback;

            OnConnectFailed += OnConnectFailedCallback;
            OnConnectSuccess += OnConnectSuccessCallback;

            OnDisconnect += OnDisconnectCallback;
        }
    }
}
