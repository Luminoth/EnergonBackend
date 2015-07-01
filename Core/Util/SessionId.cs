using System;
using System.Security.Cryptography;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class SessionId
    {
        // ReSharper disable once InconsistentNaming
        private const int IVLength = 16;

        private readonly byte[] _iv = new byte[IVLength];
        private readonly string _secret;

        // ReSharper disable once InconsistentNaming
        public int ExpiryMS { get; private set; }
        // ReSharper disable once InconsistentNaming
        public string SessionID { get; private set; }
        public long CreationTime { get; private set; }
        public bool Expired { get { return ExpiryMS >= 0 && Time.CurrentTimeMs >= (CreationTime + ExpiryMS); } }

        private SessionId()
        {
            ExpiryMS = -1;
            CreationTime = Time.CurrentTimeMs;
        }

        public SessionId(string secret) : this()
        {
            _secret = secret;

            Random random = new Random();
            long salt1 = random.Next(10000);
            long salt2 = random.Next(10000);

            string value = salt1 + ":" + CreationTime + ":" + salt2;
            string secretHash = new Crypt.SHA512().HashHexAsync(_secret).Result;

            byte[] encrypted = null;
            using(Aes aes = Aes.Create()) {
                if(null != aes) {
                    /*aes.GenerateIV();
                    Array.Copy(IV, aes.IV, Math.Min(IV.Length, aes.IV.Length));
                    encrypted = AES.EncryptAsync(value, secretHash, IV).Result;*/
                    encrypted = AES.EncryptAsync(secretHash, aes.Key, aes.IV).Result;
                }
            }

            if(null != encrypted) {
                byte[] combined = new byte[_iv.Length + encrypted.Length];
                Array.Copy(_iv, combined, _iv.Length);
                Array.Copy(encrypted, 0, combined, _iv.Length, encrypted.Length);
                SessionID = Convert.ToBase64String(combined);
            }
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
