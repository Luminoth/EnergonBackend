using System;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class Nonce
    {
        public string Realm { get; private set; }
        public int ExpiryMS { get; private set; }
        public string NonceValue { get; private set; }
        public string NonceHash { get; private set; }
        public long CreationTime { get; private set; }
        public bool Expired { get { return ExpiryMS < 0 ? false : Time.CurrentTimeMs >= (CreationTime + ExpiryMS); } }

        private Nonce()
        {
        }

        public Nonce(string realm, int expiry) : this()
        {
            Realm = realm;
            ExpiryMS = expiry;
            CreationTime = Time.CurrentTimeMs;
            NonceValue = CreationTime + ":" + Realm;
            NonceHash = new SHA512().HashHexAsync(NonceValue).Result;
        }

        public Nonce(string realm, string nonce, int expiry) : this()
        {
            Realm = realm;
            ExpiryMS = expiry;
            NonceValue = nonce;
            NonceHash = new SHA512().HashHexAsync(NonceValue).Result;

            int idx = NonceValue.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            CreationTime = int.Parse(NonceValue.Substring(0, idx));
        }
    }
}
