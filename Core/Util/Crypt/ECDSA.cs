using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public static class ECDSA
    {
        public static async Task<byte[]> SignAsync(string data, CngKey privKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng(privKey)) {
                dsa.HashAlgorithm = algorithm;
                try {
                    return await Task.Run(() => dsa.SignData(Encoding.UTF8.GetBytes(data))).ConfigureAwait(false);
                } catch(AggregateException e) {
                    throw e.InnerException;
                }
            }
        }

        public static async Task<bool> VerifyAsync(byte[] data, byte[] signature, CngKey pubKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng()) {
                dsa.HashAlgorithm = algorithm;
                try {
                    return await Task.Run(() => dsa.VerifyData(data, signature)).ConfigureAwait(false);
                } catch(AggregateException e) {
                    throw e.InnerException;
                }
            }
        }
    }
}
