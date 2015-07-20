using System;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    /// <summary>
    /// SHA512 digester
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SHA512 : Digest
    {
        public async override Task<byte[]> HashAsync(string value)
        {
            using(System.Security.Cryptography.SHA512 hasher = System.Security.Cryptography.SHA512.Create()) {
                // await is necessary here to avoid disposing of the hasher object
                // ReSharper disable once AccessToDisposedClosure
                return await Task.Run(() => hasher.ComputeHash(Encoding.UTF8.GetBytes(value))).ConfigureAwait(false);
            }
        }
    }
}
