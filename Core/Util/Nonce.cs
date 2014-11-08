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

        public bool Expired
        {
            get
            {
                if(ExpiryMS < 0) {
                    return false;
                }
                return Time.CurrentTimeMs >= (CreationTime + ExpiryMS);
            }
        }

        public Nonce(string realm, int expiry)
        {
            Realm = realm;
            ExpiryMS = expiry;
            CreationTime = Time.CurrentTimeMs;
            NonceValue = CreationTime + ":" + Realm;
            NonceHash = new SHA512().HashHex(NonceValue);
        }

        public Nonce(string realm, string nonce, int expiry)
        {
            Realm = realm;
            ExpiryMS = expiry;
            NonceValue = nonce;
            NonceHash = new SHA512().HashHex(NonceValue);

            int idx = NonceValue.IndexOf(":");
            CreationTime = int.Parse(NonceValue.Substring(0, idx));
        }
    }
}
