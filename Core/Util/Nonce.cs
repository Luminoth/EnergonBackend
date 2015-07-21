using System;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// A number used once
    /// </summary>
    [Serializable]
    public sealed class Nonce
    {
        /// <summary>
        /// Gets the nonce realm.
        /// </summary>
        /// <value>
        /// The nonce realm.
        /// </value>
        public string Realm { get; }

        /// <summary>
        /// Gets the expiration in milliseconds.
        /// </summary>
        /// <value>
        /// The expiration in milliseconds.
        /// </value>
        public int ExpiryMs { get; }

        /// <summary>
        /// Gets the nonce value.
        /// </summary>
        /// <value>
        /// The nonce value.
        /// </value>
        public string NonceValue { get; }

        /// <summary>
        /// Gets the nonce hash.
        /// </summary>
        /// <value>
        /// The nonce hash.
        /// </value>
        public string NonceHash { get; private set; }

        /// <summary>
        /// Gets the nonce creation time.
        /// </summary>
        /// <value>
        /// The nonce creation time.
        /// </value>
        public DateTime CreationTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Nonce"/> is expired.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expired; otherwise, <c>false</c>.
        /// </value>
        public bool Expired => ExpiryMs >= 0 && (DateTime.Now.Subtract(CreationTime).Milliseconds > ExpiryMs);

        /// <summary>
        /// Initializes a new instance of the <see cref="Nonce" /> class.
        /// </summary>
        /// <param name="realm">The nonce realm.</param>
        /// <param name="expiryMs">The expiration time in milliseconds.</param>
        public Nonce(string realm, int expiryMs)
            : this()
        {
            Realm = realm;
            ExpiryMs = expiryMs;
            NonceValue = $"{CreationTime.GetTicksMs()}:${Realm}";
            NonceHash = new SHA512().HashHexAsync(NonceValue).Result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nonce"/> class.
        /// </summary>
        /// <param name="realm">The nonce realm.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="expiryMs">The expiration time in milliseconds.</param>
        public Nonce(string realm, string nonce, int expiryMs)
            : this()
        {
            Realm = realm;
            ExpiryMs = expiryMs;
            NonceValue = nonce;
            NonceHash = new SHA512().HashHexAsync(NonceValue).Result;

            int idx = NonceValue.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            CreationTime = DateTimeExtensions.DateTimeFromTicksMs(int.Parse(NonceValue.Substring(0, idx)));
        }

        private Nonce()
        {
        }
    }
}
