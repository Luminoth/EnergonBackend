using System.IO;
using System.Security.Cryptography;

namespace EnergonSoftware.Core.Util.Crypt
{
    public static class AES
    {
        public static byte[] Encrypt(string data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create()) {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        using(StreamWriter writer = new StreamWriter(cs)) {
                            writer.Write(data);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public static string Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create()) {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                        using(StreamReader reader = new StreamReader(cs)) {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
