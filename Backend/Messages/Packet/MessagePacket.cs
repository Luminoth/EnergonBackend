using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages.Packet
{
    [Serializable]
    public abstract class MessagePacket : IComparable
    {
#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public abstract string Type { get; }

        public int Id { get; protected set; }

        public IMessage Content { get; set; }

        // NOTE: these can throw MessageException
        public abstract Task SerializeAsync(Stream stream, string formatterType);
        public abstract Task DeSerializeAsync(Stream stream, IMessageFactory messageFactory);

        public int CompareTo(object obj)
        {
            MessagePacket rhs = obj as MessagePacket;
            if(null == rhs) {
                return 0;
            }

            return (int)(Id - rhs.Id);
        }

        public override bool Equals(object obj)
        {
            if(null == obj) {
                return false;
            }

            MessagePacket packet = obj as MessagePacket;
            if(null == packet) {
                return false;
            }

            return Type == packet.Type && Id == packet.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected MessagePacket()
        {
            Id = NextId;
        }

        protected async Task SerializeContentAsync(IMessageFormatter formatter)
        {
            if(null == Content) {
                return;
            }

            await formatter.StartDocumentAsync().ConfigureAwait(false);
            await Content.SerializeAsync(formatter).ConfigureAwait(false);
            await formatter.EndDocumentAsync().ConfigureAwait(false);
            await formatter.FlushAsync().ConfigureAwait(false);
        }

        protected async Task DeSerializeContentAsync(IMessageFormatter formatter)
        {
            if(null == Content) {
                return;
            }

            await Content.DeSerializeAsync(formatter).ConfigureAwait(false);
        }
    }
}
