using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core
{
    public enum AuthType
    {
        [Description("NONE")]
        None,

        [Obsolete]
        [Description("DIGEST-MD5")]
        DigestMD5,

        [Description("DIGEST-SHA512")]
        DigestSHA512,
    }

    public static class Auth
    {
        public static Dictionary<string, string> ParseDigestValues(string message)
        {
            string[] pairs = message.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach(string pair in pairs) {
                int idx = pair.IndexOf("=");
                if(idx < 0) {
                    return null;
                }
                values[pair.Substring(0, idx)] = pair.Substring(idx+1);
            }
            return values;
        }

        public static async Task<string> DigestClientResponse(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestURI)
        {
            string a1 = passwordHash + ":" + nonce + ":" + cnonce;
            string ha1 = await Task.Run(() => digest.HashHex(a1));

            string a2 = "AUTHENTICATE:" + digestURI;
            string ha2 = await Task.Run(() => digest.HashHex(a2));

            string s = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2;
            string k = ha1 + ":" + s;

            return await Task.Run(() => digest.HashHex(k));
        }

        public static async Task<string> DigestServerResponse(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestURI)
        {
            string a1 = passwordHash + ":" + nonce + ":" + cnonce;
            string ha1 = await Task.Run(() => digest.HashHex(a1));

            string a2 = ":" + digestURI;
            string ha2 = await Task.Run(() => digest.HashHex(a2));

            string s = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2;
            string k = ha1 + ":" + s;

            return await Task.Run(() => digest.HashHex(k));
        }
    }
}
