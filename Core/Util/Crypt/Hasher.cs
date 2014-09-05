using System.Text;

namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Hasher
    {
        public abstract byte[] Hash(string value);

        public string HashHex(string value)
        {
            byte[] digest = Hash(value);

            StringBuilder builder = new StringBuilder();
            foreach(byte b in digest) {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
