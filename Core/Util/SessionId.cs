using System;
using System.Security.Cryptography;
using System.Text;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    public sealed class SessionId
    {
        public string Secret { get; private set; }
        public int ExpiryMS { get; private set; }
        public string SessionID { get; private set; }
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

        public SessionId(string secret, int expiry=-1)
        {
            Secret = secret;
            ExpiryMS = expiry;
            CreationTime = Time.CurrentTimeMs;

            Random random = new Random();
            long salt1 = random.Next(10000);
            long salt2 = random.Next(10000);

            string value = salt1.ToString() + ":" + CreationTime + ":" + salt2;
            string passwordHash = new EnergonSoftware.Core.Util.Crypt.SHA512().HashHex(Secret);

            byte[] encrypted = null;
            using(Aes aes = Aes.Create()) {
                encrypted = AES.Encrypt(passwordHash, aes.Key, aes.IV);
            }
            SessionID = Convert.ToBase64String(encrypted);
        }

        public SessionId(string password, string sessionId, int expiry=-1)
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
