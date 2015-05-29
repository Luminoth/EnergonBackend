using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Packet
{
    public static class PacketFactory
    {
        public static IPacket Create(string type)
        {
            switch(type)
            {
            case NetworkPacket.PacketType:
                return new NetworkPacket();
            }

            throw new ArgumentException("Invalid packet type", "type");
        }

        public static async Task<IPacket> CreateAsync(Stream stream)
        {
            if(stream.GetRemaining() >= NetworkPacket.Header.Length) {
                byte[] header = new byte[NetworkPacket.Header.Length];
                await stream.PeekAsync(header, 0, header.Length).ConfigureAwait(false);
                if(header.SequenceEqual(NetworkPacket.Header)) {
                    return new NetworkPacket();
                }
            }

            return null;
        }
    }
}
