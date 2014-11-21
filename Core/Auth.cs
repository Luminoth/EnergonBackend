using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        public static string DigestClientResponse(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestURI)
        {
            string A1 = passwordHash + ":" + nonce + ":" + cnonce;
            string HA1 = digest.HashHex(A1);

            string A2 = "AUTHENTICATE:" + digestURI;
            string HA2 = digest.HashHex(A2);

            string S = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + HA2;
            string k = HA1 + ":" + S;

            return digest.HashHex(k);
        }

        public static string DigestServerResponse(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestURI)
        {
            string A1 = passwordHash + ":" + nonce + ":" + cnonce;
            string HA1 = digest.HashHex(A1);

            string A2 = ":" + digestURI;
            string HA2 = digest.HashHex(A2);

            string S = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + HA2;
            string k = HA1 + ":" + S;

            return digest.HashHex(k);
        }
    }
}
