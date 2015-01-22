using System;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    [Obsolete("Use SHA512 instead")]
    public class MD5 : Digest
    {
        public async override Task<byte[]> HashAsync(string value)
        {
            using(System.Security.Cryptography.MD5 hasher = System.Security.Cryptography.MD5.Create()) {
                try {
                    return await Task.Run(() => hasher.ComputeHash(Encoding.UTF8.GetBytes(value))).ConfigureAwait(false);
                } catch(AggregateException e) {
                    throw e.InnerException;
                }
            }
        }
    }
}
