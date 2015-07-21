using System;
using System.Security.Cryptography;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// Represents a secure SessionId
    /// </summary>
    [Serializable]
    public sealed class SessionId
    {
        // ReSharper disable once InconsistentNaming
        private const int IVLength = 16;

        private readonly byte[] _iv = new byte[IVLength];
        private readonly string _secret;

        /// <summary>
        /// Gets the expiration in milliseconds.
        /// </summary>
        /// <value>
        /// The expiration in milliseconds.
        /// </value>
        public int ExpiryMs { get; } = -1;

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        // ReSharper disable once InconsistentNaming
        public string SessionID { get; private set; }

        /// <summary>
        /// Gets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTime CreationTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SessionId"/> is expired.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expired; otherwise, <c>false</c>.
        /// </value>
        public bool Expired => ExpiryMs >= 0 && (DateTime.Now.Subtract(CreationTime).Milliseconds > ExpiryMs);

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionId"/> class.
        /// </summary>
        /// <param name="secret">The secret.</param>
        public SessionId(string secret)
            : this()
        {
            _secret = secret;

            Random random = new Random();
            long salt1 = random.Next(10000);
            long salt2 = random.Next(10000);

            string value = $"{salt1}:{CreationTime.GetTicksMs()}:{salt2}";
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionId" /> class.
        /// </summary>
        /// <param name="secret">The secret.</param>
        /// <param name="expiryMs">The expiration in milliseconds.</param>
        public SessionId(string secret, int expiryMs)
            : this(secret)
        {
            ExpiryMs = expiryMs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionId"/> class.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public SessionId(string password, string sessionId)
            : this()
        {
            SessionID = sessionId;
throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionId"/> class.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="expiryMs">The expiration in milliseconds.</param>
        public SessionId(string password, string sessionId, int expiryMs)
            : this(password, sessionId)
        {
            ExpiryMs = expiryMs;
        }

        private SessionId()
        {
        }
    }
}
