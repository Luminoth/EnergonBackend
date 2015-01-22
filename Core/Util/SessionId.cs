using System;
using System.Security.Cryptography;
using System.Text;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class SessionId
    {
        private const int IVLength = 16;

        private readonly byte[] IV = new byte[IVLength];
        private readonly string Secret;

        public readonly int ExpiryMS = -1;
        public readonly string SessionID;
        public readonly long CreationTime = Time.CurrentTimeMs;
        public bool Expired { get { return ExpiryMS < 0 ? false : Time.CurrentTimeMs >= (CreationTime + ExpiryMS); } }

        private SessionId()
        {
        }

        public SessionId(string secret) : this()
        {
            Secret = secret;

            Random random = new Random();
            long salt1 = random.Next(10000);
            long salt2 = random.Next(10000);

            string value = salt1.ToString() + ":" + CreationTime + ":" + salt2.ToString();
            byte[] secretHash = Encoding.UTF8.GetBytes(new EnergonSoftware.Core.Util.Crypt.SHA512().HashHexAsync(Secret).Result);

            byte[] encrypted = null;
            using(Aes aes = Aes.Create()) {
                aes.GenerateIV();
                Array.Copy(IV, aes.IV, Math.Min(IV.Length, aes.IV.Length));
                encrypted = AES.EncryptAsync(value, secretHash, IV).Result;
            }

            byte[] combined = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, combined, IV.Length);
            Array.Copy(encrypted, 0, combined, IV.Length, encrypted.Length);
            SessionID = Convert.ToBase64String(combined);
        }

        public SessionId(string secret, int expiry) : this(secret)
        {
            ExpiryMS = expiry;
        }

        public SessionId(string password, string sessionId) : this()
        {
            SessionID = sessionId;
            throw new NotImplementedException();
        }

        public SessionId(string password, string sessionId, int expiry) : this(password, sessionId)
        {
            ExpiryMS = expiry;
        }
    }
}
