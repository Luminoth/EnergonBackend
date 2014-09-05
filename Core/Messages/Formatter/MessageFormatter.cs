using System.IO;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageFormatter
    {
        void WriteString(string value, Stream stream);
        void WriteByte(byte value, Stream stream);
        void WriteBool(bool value, Stream stream);
        void WriteInt(int value, Stream stream);
        void WriteLong(long value, Stream stream);
        void WriteFloat(float value, Stream stream);
        void WriteDouble(double value, Stream stream);
        void Write(byte[] value, int offset, int count, Stream stream);

        string ReadString(Stream stream);
        byte ReadByte(Stream stream);
        bool ReadBool(Stream stream);
        int ReadInt(Stream stream);
        long ReadLong(Stream stream);
        float ReadFloat(Stream stream);
        double ReadDouble(Stream stream);
        void Read(byte[] value, int offset, int count, Stream stream);
    }
}
