using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    public enum AuthType
    {
        [Description("NONE")]
        None,

        [Obsolete("Use DIGEST-SHA512 instead")]
        [Description("DIGEST-MD5")]
        // ReSharper disable once InconsistentNaming
        DigestMD5,

        [Description("DIGEST-SHA512")]
        // ReSharper disable once InconsistentNaming
        DigestSHA512,
    }

    public static class AuthUtil
    {
        public static Dictionary<string, string> ParseDigestValues(string message)
        {
            if(null == message) {
                throw new ArgumentNullException("message");
            }

            string[] pairs = message.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach(string pair in pairs) {
                int idx = pair.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
                if(idx < 0) {
                    return null;
                }

                values[pair.Substring(0, idx)] = pair.Substring(idx + 1);
            }

            return values;
        }

        public static async Task<string> DigestClientResponseAsync(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestUri)
        {
            string a1 = passwordHash + ":" + nonce + ":" + cnonce;
            string ha1 = await digest.HashHexAsync(a1).ConfigureAwait(false);

            string a2 = "AUTHENTICATE:" + digestUri;
            string ha2 = await digest.HashHexAsync(a2).ConfigureAwait(false);

            string s = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2;
            string k = ha1 + ":" + s;

            return await digest.HashHexAsync(k).ConfigureAwait(false);
        }

        public static async Task<string> DigestServerResponseAsync(Digest digest, string passwordHash, string nonce, string nc, string qop, string cnonce, string digestUri)
        {
            string a1 = passwordHash + ":" + nonce + ":" + cnonce;
            string ha1 = await digest.HashHexAsync(a1).ConfigureAwait(false);

            string a2 = ":" + digestUri;
            string ha2 = await digest.HashHexAsync(a2).ConfigureAwait(false);

            string s = nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2;
            string k = ha1 + ":" + s;

            return await digest.HashHexAsync(k).ConfigureAwait(false);
        }
    }
}
