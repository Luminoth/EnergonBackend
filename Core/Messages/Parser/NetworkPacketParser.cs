using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Messages.Parser
{
    public sealed class NetworkPacketParser : IMessagePacketParser, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkPacketParser));

#region Events
        public event EventHandler<MessageParsedEventArgs> MessageParsedEvent;
#endregion

        private SemaphoreSlim _streamLock = new SemaphoreSlim(1);

        private MemoryStream _stream = new MemoryStream();

        public NetworkPacketParser()
        {
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                _streamLock.Dispose();
                _stream.Dispose();
            }
        }
#endregion

        // TODO: does this go away?
        public MessagePacket Create()
        {
            return new NetworkPacket();
        }

#region Event Handlers
        public async void DataReceivedEventHandlerAsync(object sender, DataReceivedEventArgs e)
        {
            await _streamLock.WaitAsync().ConfigureAwait(false);
            try {
                Logger.Debug("Read network data (" + e.Count + "):");
                Logger.Debug(Utils.HexDump(e.Data, 0, e.Count));

                await _stream.WriteAsync(e.Data, 0, e.Count);
            } finally {
                _streamLock.Release();
            }
        }
#endregion

        public async Task ParseAsync()
        {
// TODO: loop!
            await _streamLock.WaitAsync().ConfigureAwait(false);

            try {
                _stream.Flip();
                if(!_stream.HasRemaining()) {
                    _stream.Reset();
                    return;
                }

                NetworkPacket packet = new NetworkPacket();
                try {
                    await packet.DeSerializeAsync(_stream).ConfigureAwait(false);
                } catch(MessageException e) {
                    Logger.Warn(Resources.ErrorParsingNetworkPacket, e);
                    _stream.Reset();
                    return;
                }

                await _stream.CompactAsync().ConfigureAwait(false);

                if(null != MessageParsedEvent) {
                    MessageParsedEvent(this, new MessageParsedEventArgs() { Message = packet.Content });
                }
            } finally {
                _streamLock.Release();
            }
        }
    }
}
