using System;
using System.Text;

namespace EnergonSoftware.Core.Util
{
    public static class Util
    {
        public static string HexDump(byte[] buffer, int offset, int count)
        {
            const int GroupCount = 2;
            const int BytesPerGroup = 4;
            const int BytesPerLine = BytesPerGroup * GroupCount;

            int length = Math.Min(offset + count, buffer.Length);
            int rowCount = (int)Math.Ceiling((decimal)length / (decimal)BytesPerLine);

            StringBuilder builder = new StringBuilder();
            for(int row=0; row<rowCount; ++row) {
                int start = offset + (row * BytesPerLine);
                int end = Math.Min(start + BytesPerLine, length);

                // hex
                for(int j=start; j<start + BytesPerLine; ++j) {
                    if(j < end) {
                        builder.Append(string.Format("{0:X2} ", buffer[j]));
                    } else {
                        builder.Append("   ");
                    }
                }

                builder.Append(" ");

                // ascii
                for(int j=start; j<end; ++j) {
                    char ch = (char)buffer[j];
                    builder.Append(string.Format("{0} ", Char.IsControl(ch) ? '.' : ch));
                }

                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }
}
