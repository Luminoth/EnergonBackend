using System;
using System.Configuration;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Overmind.MessageHandlers;

namespace EnergonSoftware.Overmind.Net
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

        public Session(Socket socket)
        {
            Id = NextId;

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
                if(Socket.Connected) {
                    _logger.Info("Session " + Id + " disconnecting...");
                    _socketState.ShutdownAndClose(true);
                }
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
    }
}
