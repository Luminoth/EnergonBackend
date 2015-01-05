using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public class SHA512 : Digest
    {
        public async override Task<byte[]> HashAsync(string value)
        {
            using(System.Security.Cryptography.SHA512 hasher = System.Security.Cryptography.SHA512.Create()) {
                return await Task.Run(() => hasher.ComputeHash(Encoding.UTF8.GetBytes(value))).ConfigureAwait(false);
            }
        }
    }
}
