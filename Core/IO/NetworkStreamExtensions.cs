using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class NetworkStreamExtensions
    {
#region Write
        public static async Task WriteNetworkAsync(this Stream stream, string value)
        {
            await WriteNetworkAsync(stream, value, Encoding.UTF8).ConfigureAwait(false);
        }

        public static async Task WriteNetworkAsync(this Stream stream, string value, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(value);
            await stream.WriteNetworkAsync(bytes.Length).ConfigureAwait(false);
            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public static async Task WriteNetworkAsync(this Stream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public static async Task WriteNetworkAsync(this Stream stream, long value)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public static async Task WriteNetworkAsync(this Stream stream, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public static async Task WriteNetworkAsync(this Stream stream, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
#endregion

#region Read
        public static async Task<string> ReadNetworkStringAsync(this Stream stream)
        {
            int length = await stream.ReadNetworkIntAsync().ConfigureAwait(false);
            byte[] bytes = new byte[length];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        public static async Task<int> ReadNetworkIntAsync(this Stream stream)
        {
            byte[] bytes = new byte[4];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }

        public static async Task<long> ReadNetworkLongAsync(this Stream stream)
        {
            byte[] bytes = new byte[8];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, 0));
        }

        public static async Task<float> ReadNetworkFloatAsync(this Stream stream)
        {
            byte[] bytes = new byte[4];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return BitConverter.ToSingle(bytes, 0);
        }

        public static async Task<double> ReadNetworkDoubleAsync(this Stream stream)
        {
            byte[] bytes = new byte[8];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return BitConverter.ToDouble(bytes, 0);
        }
#endregion
    }
}
