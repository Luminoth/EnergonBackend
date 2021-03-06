﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.IO
{
    /// <summary>
    /// Useful extensions to the System.IO.Stream class.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Finds the index of the first occurrence of the given byte value.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value to look for.</param>
        /// <returns>
        /// The index of the first occurrence of the given byte value.
        /// </returns>
        public static async Task<long> IndexOfAsync(this Stream stream, byte[] value)
        {
            if(null == value) {
                return -1;
            }

            long startPosition = stream.Position;
            long currentPosition = startPosition;

            long index = -1;
            while(currentPosition < stream.Length) {
                byte[] buffer = new byte[value.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if(buffer.SequenceEqual(value)) {
                    index = currentPosition;
                    break;
                }

                stream.Position = ++currentPosition;
            }

            stream.Position = startPosition;
            return index;
        }

        /// <summary>
        /// Consumes the given number of bytes from the stream without returning them.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <returns>The number of bytes actually consumed.</returns>
        public static async Task<int> ConsumeAsync(this Stream stream, int count)
        {
            if(count <= 0) {
                return 0;
            }

            byte[] consumed = new byte[count];
            return await stream.ReadAsync(consumed, 0, consumed.Length).ConfigureAwait(false);
        }

#region Write
        /// <summary>
        /// Writes a single byte to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The byte to write.</param>
        public static Task WriteByteAsync(this Stream stream, byte value)
        {
            return Task.Run(() => stream.WriteByte(value));
        }

        /// <summary>
        /// Writes a single byte to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The byte to write.</param>
        public static async Task WriteAsync(this Stream stream, byte value)
        {
            await stream.WriteByteAsync(value).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes a byte-size boolean value to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">If set to <c>true</c> a 1 is written, otherwise a 0 is written.</param>
        public static async Task WriteAsync(this Stream stream, bool value)
        {
            await stream.WriteAsync((byte)(value ? 1 : 0)).ConfigureAwait(false);
        }
#endregion

#region Peek
        /// <summary>
        /// Peeks at a set of bytes in the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer to write the bytes into.</param>
        /// <param name="offset">The offset into the buffer to start at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes actually peeked at.</returns>
        public static async Task<int> PeekAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            int read = await stream.ReadAsync(buffer, offset, count).ConfigureAwait(false);
            stream.Seek(-read, SeekOrigin.Current);
            return read;
        }
#endregion

#region Read
        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The byte that was read.</returns>
        public static Task<int> ReadByteAsync(this Stream stream)
        {
            return Task.Run(() => stream.ReadByte());
        }

        /// <summary>
        /// Reads a byte-size boolean value from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The boolean value that was read.</returns>
        public static async Task<bool> ReadBoolAsync(this Stream stream)
        {
            return 0 != await stream.ReadByteAsync().ConfigureAwait(false);
        }
#endregion

#region Network Write
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

#region Network Peek
        public static async Task<int> PeekNetworkIntAsync(this Stream stream)
        {
            byte[] bytes = new byte[4];
            await stream.PeekAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }
#endregion

#region Network Read
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

#region Buffer
        /// <summary>
        /// Gets the remaining number of bytes in the stream that can be read.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The number of bytes between the current position and the end of the stream.</returns>
        public static long GetRemaining(this Stream stream)
        {
            return stream.Length - stream.Position;
        }

        /// <summary>
        /// Determines whether the stream has remaining bytes.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>True of the stream has remaining bytes, otherwise false.</returns>
        public static bool HasRemaining(this Stream stream)
        {
            return stream.GetRemaining() > 0;
        }

        /// <summary>
        /// Sets the position to 0 and the length to 0.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static void Clear(this Stream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
        }

        /// <summary>
        /// Sets the length to the position and the position to 0.
        /// Call this to swap from write operations to read operations.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static void Flip(this Stream stream)
        {
            stream.SetLength(stream.Position);
            stream.Position = 0;
        }

        /// <summary>
        /// Sets the position to the length.
        /// Call this to swap from read operations to write operations.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static void Reset(this Stream stream)
        {
            stream.Position = stream.Length;
        }
#endregion
    }
}
