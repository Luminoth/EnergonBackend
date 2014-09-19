using System;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class Nonce
    {
        private string _realm;
        private int _expiry;
        private string _nonce;
        private string _nonceHash;
        private long _creationTime;

        public string Realm { get { return _realm; } }
        public int ExpiryMS { get { return _expiry; } }
        public string NonceValue { get { return _nonce; } }
        public string NonceHash { get { return _nonceHash; } }
        public long CreationTime { get { return _creationTime; } }
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
            _realm = realm;
            _expiry = expiry;
            _creationTime = Time.CurrentTimeMs;
            _nonce = CreationTime + ":" + Realm;
            _nonceHash = new SHA512().HashHex(NonceValue);
        }

        public Nonce(string realm, string nonce, int expiry)
        {
            _realm = realm;
            _expiry = expiry;
            _nonce = nonce;
            _nonceHash = new SHA512().HashHex(NonceValue);

            int idx = NonceValue.IndexOf(":");
            _creationTime = int.Parse(NonceValue.Substring(0, idx));
        }
    }
}
