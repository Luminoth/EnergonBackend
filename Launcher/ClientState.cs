using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

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

    sealed class ClientState
    {
#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

        private object _lock = new object();

#region Network Events
        public delegate void OnConnectSuccessHandler();
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler();
        public event OnDisconnectHandler OnDisconnect;
#endregion

#region Authentication Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        public delegate void OnLogoutHandler();
        public event OnLogoutHandler OnLogout;

#region Network Properties
        private volatile bool _connecting = false;
        private volatile string _host;
        private volatile Socket _socket;
        private volatile BufferedSocketReader _reader;
        private volatile IMessageFormatter _formatter = new BinaryMessageFormatter();

        public string Host { get { return _host; } }

        public bool Connecting
        {
            get { return _connecting; }
            private set
            {
                lock(_lock) {
                    _connecting = value;
                    NotifyPropertyChanged("Connecting");
                    NotifyPropertyChanged("Connected");
                }
            } 
        }

        public bool Connected { get { return null != _socket && _socket.Connected; } }
#endregion

#region Authentication Properties
        private volatile AuthenticationStage _authStage = AuthenticationStage.NotAuthenticated;

        private volatile string _username;
        private volatile string _password;

        internal AuthenticationStage AuthStage
        {
            get { return _authStage; }
            private set
            {
                lock(_lock) {
                    _authStage = value;
                    NotifyPropertyChanged("Authenticating");
                    NotifyPropertyChanged("Authenticated");
                }
            }
        }

        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        public string Username { get { return _username; } }
        internal string Password { get { return _password; } }
#endregion

#region UI Helpers
        public bool CanLogin { get { return !Connecting && !Connected && !Authenticating && !Authenticated; } }
#endregion

#region Network Methods
        private void OnConnectAsyncFailed(SocketError error)
        {
            lock(_lock) {
                _logger.Error("Connect failed: " + error);

                _host = null;
                Connecting = false;
            }

            if(null != OnConnectFailed) {
                OnConnectFailed(error);
            }

            OnConnectFailed = null;
            OnConnectSuccess = null;
        }

        private void OnConnectAsyncSuccess(Socket socket)
        {
            lock(_lock) {
                _logger.Info("Connected to " + socket.RemoteEndPoint);

                _socket = socket;
                _reader = new BufferedSocketReader(_socket);
                Connecting = false;
            }

            if(null != OnConnectSuccess) {
                OnConnectSuccess();
            }

            OnConnectFailed = null;
            OnConnectSuccess = null;
        }

        public void ConnectAsync(string host, int port)
        {
            lock(_lock) {
                Disconnect();

                _host = host;
                _logger.Info("Connecting to " + Host + ":" + port + "...");

                Connecting = true;
            }

            AsyncConnectEventArgs args = new AsyncConnectEventArgs();
            args.OnConnectFailed += OnConnectAsyncFailed;
            args.OnConnectSuccess += OnConnectAsyncSuccess;
            NetUtil.ConnectAsync(host, port, args);
        }

        public void Disconnect()
        {
            if(null != _socket) {
                lock(_lock) {
                    _logger.Info("Disconnecting...");
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                    _socket.Close();
                    _socket = null;
                    _reader = null;
                }

                if(null != OnDisconnect) {
                    OnDisconnect();
                }
            }

            lock(_lock) {
                _host = null;
                Connecting = false;
                AuthStage = AuthenticationStage.NotAuthenticated;
            }
        }

        private bool Poll()
        {
            if(!Connected) {
                return false;
            }

            lock(_lock) {
                // TODO: this needs to be a while loop
                try {
                    if(_socket.Poll(100, SelectMode.SelectRead)) {
                        int len = _reader.Read();
                        if(0 == len) {
                            Error("End of stream!");
                            return false;
                        }
                        _logger.Debug("Read " + len + " bytes");
                    }
                } catch(SocketException e) {
                    Error(e);
                    return false;
                }
            }

            return true;
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
                _logger.Debug("Sending " + bytes.Length + " bytes");
                _socket.Send(bytes);
            }
        }
#endregion

#region Authentication Methods
        public void BeginAuth(string username, string password)
        {
            lock(_lock) {
                Logout();

                _username = username;
                _password = password;
                _logger.Info("Authenticating as " + Username + "...");

                AuthMessage message = new AuthMessage();
                message.MechanismType = AuthType.DigestSHA512;
                SendMessage(message);

                AuthStage = AuthenticationStage.Begin;
            }
        }

        public void Logout()
        {
            /*lock(_lock) {
                _logging.Info("Logging out...");
                LogoutMessage message = new LogoutMessage();
                SendMessage(message);

                if(null != OnLogout) {
                    OnLogout();
                }

                AuthStage = AuthenticationStage.NotAuthenticated;
                _username = null;
                _password = null;
                _rspAuth = null;
                _sessionid = null;

                _account = new Account();
            }*/
        }
#endregion

        private bool HandleMessages()
        {
            lock(_lock) {
                // TODO: this needs to be a while loop
                try {
                    NetworkMessage message = NetworkMessage.Parse(_reader.Buffer, _formatter);
                    if(null != message) {
                        _logger.Debug("Parsed message type: " + message.Payload.Type);
                        MessageHandler.HandleMessage(message.Payload);
                    }
                } catch(Exception e) {
                    Error(e);
                    return false;
                }
                return true;
            }
        }

        public void Run()
        {
            if(!Poll()) {
                return;
            }

            if(!HandleMessages()) {
                return;
            }

            //Ping();
        }

        public void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);
            Disconnect();
        }

        public void Error(Exception error)
        {
            Error(error.Message);
        }

#region Property Notifier
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if("Authenticated".Equals(e.PropertyName, StringComparison.InvariantCultureIgnoreCase)) {
                NotifyPropertyChanged("NotAuthenticated");
            }

            NotifyPropertyChanged("CanLogin");
            NotifyPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
        }
    }
}
