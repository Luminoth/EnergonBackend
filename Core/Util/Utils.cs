using System;
using System.IO;
using System.Text;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// Useful utility methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Dumps a buffer to a hexadecimal output string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The buffer as a hexadecimal output string</returns>
        public static string HexDump(byte[] buffer, int offset, int count)
        {
            const int groupCount = 2;
            const int bytesPerGroup = 4;
            const int bytesPerLine = bytesPerGroup * groupCount;

            int length = Math.Min(offset + count, buffer.Length);
            int rowCount = (int)Math.Ceiling(length / (decimal)bytesPerLine);

            StringBuilder builder = new StringBuilder();
            for(int row = 0; row < rowCount; ++row) {
                int start = offset + (row * bytesPerLine);
                int end = Math.Min(start + bytesPerLine, length);

                // hex
                for(int j = start; j < (start + bytesPerLine); ++j) {
                    builder.Append(j < end ? $"{buffer[j]:X2} " : "   ");
                }

                builder.Append(" ");

                // ascii
                for(int j = start; j < end; ++j) {
                    char ch = (char)buffer[j];
                    builder.Append($"{(char.IsControl(ch) ? '.' : ch)} ");
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Dumps a stream to a hexadecimal output string
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The stream as a hexadecimal output string</returns>
        public static string HexDump(MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            return HexDump(buffer, 0, buffer.Length);
        }
    }
}
