using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    /// <summary>
    /// ECDSA signator
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ECDSA
    {
        /// <summary>
        /// Generates a signature for the given data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="privKey">The private key.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <returns>The signature</returns>
        public static async Task<byte[]> SignAsync(string data, CngKey privKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng(privKey)) {
                dsa.HashAlgorithm = algorithm;

                // await is necessary here to avoid disposing of the dsa object
                // ReSharper disable once AccessToDisposedClosure
                return await Task.Run(() => dsa.SignData(Encoding.UTF8.GetBytes(data)));
            }
        }

        /// <summary>
        /// Verifies the ECDSA signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="pubKey">The public key key.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <returns>True if the signature is valid</returns>
        public static async Task<bool> VerifyAsync(byte[] data, byte[] signature, CngKey pubKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng()) {
                dsa.HashAlgorithm = algorithm;

                // await is necessary here to avoid disposing of the dsa object
                // ReSharper disable once AccessToDisposedClosure
                return await Task.Run(() => dsa.VerifyData(data, signature)).ConfigureAwait(false);
            }
        }
    }
}
