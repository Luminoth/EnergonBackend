using System;
using System.Security.Cryptography;
using System.Text;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    [Serializable]
    public sealed class SessionId
    {
        public readonly string Secret;
        public readonly int ExpiryMS;
        public readonly string SessionID;
        public readonly long CreationTime;
        public bool Expired { get { return ExpiryMS < 0 ? false : Time.CurrentTimeMs >= (CreationTime + ExpiryMS); } }

        private SessionId()
        {
        }

        public SessionId(string secret, int expiry = -1) : this()
        {
            Secret = secret;
            ExpiryMS = expiry;
            CreationTime = Time.CurrentTimeMs;

            Random random = new Random();
            long salt1 = random.Next(10000);
            long salt2 = random.Next(10000);

            string value = salt1.ToString() + ":" + CreationTime + ":" + salt2;
            string passwordHash = new EnergonSoftware.Core.Util.Crypt.SHA512().HashHexAsync(Secret).Result;

            byte[] encrypted = null;
            using(Aes aes = Aes.Create()) {
                encrypted = AES.EncryptAsync(passwordHash, aes.Key, aes.IV).Result;
            }
            SessionID = Convert.ToBase64String(encrypted);
        }

        public SessionId(string password, string sessionId, int expiry = -1) : this()
        {
/*
 *     // unencode the value
    size_t len = 0;
    boost::shared_ptr<unsigned char> decoded(base64_decode(this->sessionid().c_str(), len));
    // get the password md5 (our key)
    char password_md5[(MD5_DIGEST_LENGTH << 1) + 1];
    md5sum_hex(reinterpret_cast<const unsigned char*>(this->password().c_str()), this->password().length(), password_md5);
    // decrypt the session id
    size_t olen = 0;
    unsigned char decrypted[1024];
    blowfish_decrypt(reinterpret_cast<const unsigned char*>(password_md5), decoded.get(), len, decrypted, olen);
    std::string value(reinterpret_cast<char*>(decrypted), olen);
    size_t pos1 = value.find(":");
    size_t pos2 = value.rfind(":");
    _creation_time = atol(value.substr(pos1+1, pos2 - pos1).c_str());
 * */
        }
    }
}
