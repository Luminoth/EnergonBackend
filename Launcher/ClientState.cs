using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher
{
    sealed class ClientState
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        public OvermindSession _overmindSession;

        private bool ShouldPing
        {
            get {
                if(null == _overmindSession) {
                    return false;
                }

                if(0 == _overmindSession.LastMessageTime) {
                    return false;
                }
                return Time.CurrentTimeMs > (_overmindSession.LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"]));
            }
        }

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
        public string Username { get { return AuthManager.Instance.Username; } set { AuthManager.Instance.Username = value; } }

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

#region Overmind Methods
        private void OnSocketErrorCallback(string error)
        {
            _overmindSession.Disconnect();
        }

        private void OnDisconnectCallback()
        {
            Error("Overmind disconnected!");
            _overmindSession = null;
            LoggedIn = false;
        }

        private void OnConnectFailedCallback(SocketError error)
        {
            Error("Failed to connect to the overmind server: " + error);
        }

        private void OnConnectSuccessCallback()
        {
            Login();
        }

        internal OvermindSession OvermindConnect()
        {
            _overmindSession = new OvermindSession();
            _overmindSession.ConnectAsync(ConfigurationManager.AppSettings["overmindHost"], Int32.Parse(ConfigurationManager.AppSettings["overmindPort"]));
            return _overmindSession;
        }

        private void Login()
        {
            _logger.Info("Logging in...");

            LoginMessage message = new LoginMessage();
            message.Username = AuthManager.Instance.Username;
            message.Ticket = AuthManager.Instance.Ticket;
            _overmindSession.SendMessage(message);

            LoggingIn = false;
            LoggedIn = true;
        }

        internal void Logout()
        {
            LogoutMessage message = new LogoutMessage();
            _overmindSession.SendMessage(message);
            _overmindSession.Disconnect();
        }

        public void Ping()
        {
            if(!ShouldPing) {
                return;
            }

            PingMessage message = new PingMessage();
            _overmindSession.SendMessage(message);
        }
#endregion

        /*private void HandleMessages(int socketId)
        {
            SocketState state = ConnectionManager.Instance.GetSocketState(socketId);
            if(null == state || !state.HasSocket) {
                return;
            }

            if(!_messages.ContainsKey(socketId)) {
                _messages[socketId]  = new Queue<IMessage>();
            }

            // cleanup the current message handler
            if(_currentMessageHandlers.ContainsKey(socketId) && _currentMessageHandlers[socketId].Finished) {
                _currentMessageHandlers.Remove(socketId);
            }

            // read off the data buffer and queue any complete messages
            NetworkMessage message = NetworkMessage.Parse(state.Buffer, _formatter);
            while(null != message) {
                _logger.Debug("Parsed message type: " + message.Payload.Type);
                _messages[socketId].Enqueue(message.Payload);
                message = NetworkMessage.Parse(state.Buffer, _formatter);
            }

            // handle the next message if we can
            if(!_currentMessageHandlers.ContainsKey(socketId) && _messages[socketId].Count > 0) {
                IMessage nextMessage = _messages[socketId].Dequeue();

                MessageHandler handler = MessageHandlerFactory.NewHandler(nextMessage.Type);
                if(null != handler) {
                    _currentMessageHandlers[socketId] = handler;
                    _currentMessageHandlers[socketId].HandleMessage(nextMessage);
                }
            }

            // TODO: we need a way to say "hey, this handler is taking WAY too long,
            // dump an error and kill the session"
        }

        private void HandleMessages()
        {
            HandleMessages(AuthSocketId);
            HandleMessages(OvermindSocketId);
        }

        private void Poll()
        {
            ConnectionManager.Instance.Poll(AuthSocketId);
            ConnectionManager.Instance.Poll(OvermindSocketId);
        }

        public void Run()
        {
            Poll();

            HandleMessages();

            Ping();
        }

        private void Reset()
        {
            LoggingIn = false;
            LoggedIn = false;
        }*/

        public delegate void OnErrorHandler(string error);
        public event OnErrorHandler OnError;

        public void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);

            if(null != OnError) {
                OnError(error);
            }
        }

        public void Error(Exception error)
        {
            Error(error.Message);
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(/*[CallerMemberName]*/ string property/*=null*/)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
            /*ConnectionManager.Instance.OnSocketError += OnSocketErrorCallback;

            ConnectionManager.Instance.OnConnectFailed += OnConnectFailedCallback;
            ConnectionManager.Instance.OnConnectSuccess += OnConnectSuccessCallback;

            ConnectionManager.Instance.OnDisconnect += OnDisconnectCallback;*/
        }
    }
}
