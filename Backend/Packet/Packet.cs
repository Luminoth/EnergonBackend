using System;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Packet
{
    public interface IPacket : IComparable
    {
        string Type { get; }

        int Id { get; }

        string ContentType { get; set; }

        string Encoding { get; set; }

        int ContentLength { get; }

        byte[] Content { get; set; }

        Task SerializeAsync(Stream stream);
        Task<bool> DeserializeAsync(Stream stream);
    }
}
