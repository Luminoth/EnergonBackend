using System;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class Nonce
    {
        public readonly string Realm;
        public readonly int ExpiryMS;
        public readonly string NonceValue;
        public readonly string NonceHash;
        public readonly long CreationTime;
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

            int idx = NonceValue.IndexOf(":");
            CreationTime = int.Parse(NonceValue.Substring(0, idx));
        }
    }
}
