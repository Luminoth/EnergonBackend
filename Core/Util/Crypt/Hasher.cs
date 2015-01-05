using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Hasher
    {
        public abstract Task<byte[]> HashAsync(string value);

        public async Task<string> HashHexAsync(string value)
        {
            byte[] digest = await HashAsync(value).ConfigureAwait(false);

            StringBuilder builder = new StringBuilder();
            foreach(byte b in digest) {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
