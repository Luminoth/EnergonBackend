using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public static class AES
    {
        public static async Task<byte[]> EncryptAsync(string data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create()) {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                MemoryStream ms = new MemoryStream();
                using(StreamWriter writer = new StreamWriter(new CryptoStream(ms, encryptor, CryptoStreamMode.Write))) {
                    await writer.WriteAsync(data).ConfigureAwait(false);
                }
                return ms.ToArray();
            }
        }

        public static async Task<string> DecryptAsync(byte[] data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create()) {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                MemoryStream ms = new MemoryStream();
                using(StreamReader reader = new StreamReader(new CryptoStream(ms, decryptor, CryptoStreamMode.Read))) {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
