using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EnergonSoftware.Core.Util.Crypt
{
    public static class ECDSA
    {
        public static byte[] Sign(string data, CngKey privKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng(privKey)) {
                dsa.HashAlgorithm = algorithm;
                return dsa.SignData(Encoding.UTF8.GetBytes(data));
            }
        }

        public static bool Verify(byte[] data, byte[] signature, CngKey pubKey, CngAlgorithm algorithm)
        {
            using(ECDsaCng dsa = new ECDsaCng()) {
                dsa.HashAlgorithm = algorithm;
                return dsa.VerifyData(data, signature);
            }
        }
    }
}
