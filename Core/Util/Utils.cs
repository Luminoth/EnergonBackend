using System;
using System.Text;

namespace EnergonSoftware.Core.Util
{
    public static class Utils
    {
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
                    builder.Append(j < end ? string.Format("{0:X2} ", buffer[j]) : "   ");
                }

                builder.Append(" ");

                // ascii
                for(int j = start; j < end; ++j) {
                    char ch = (char)buffer[j];
                    builder.Append(string.Format("{0} ", char.IsControl(ch) ? '.' : ch));
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}
