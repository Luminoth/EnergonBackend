using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    /// <summary>
    /// Hashes values
    /// </summary>
    public abstract class Hasher
    {
        /// <summary>
        /// Hashes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The hash</returns>
        public abstract Task<byte[]> HashAsync(string value);

        /// <summary>
        /// Hashes the value and converts it to a hexadecimal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The hash value as a hexadecimal string</returns>
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
