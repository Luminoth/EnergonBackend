using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// Authentication types
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Don't use this
        /// </summary>
        [Description("NONE")]
        None,

        /// <summary>
        /// Digest-MD5 authentication
        /// </summary>
        [Obsolete("Use DIGEST-SHA512 instead")]
        [Description("DIGEST-MD5")]
        // ReSharper disable once InconsistentNaming
        DigestMD5,

        /// <summary>
        /// Digest-SHA512 authentication
        /// </summary>
        [Description("DIGEST-SHA512")]
        // ReSharper disable once InconsistentNaming
        DigestSHA512,
    }

    /// <summary>
    /// Useful authentication methods
    /// </summary>
    public static class AuthUtil
    {
        /// <summary>
        /// Parses digest values into a dictionary.
        /// This assumes the pairs are comma separated.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>The digest values in a dictionary</returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
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

        /// <summary>
        /// Digests the client response.
        /// </summary>
        /// <param name="digest">The digest to use.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="nc">The nc.</param>
        /// <param name="qop">The qop.</param>
        /// <param name="cnonce">The cnonce.</param>
        /// <param name="digestUri">The digest URI.</param>
        /// <returns>The digest</returns>
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

        /// <summary>
        /// Digests the server response.
        /// </summary>
        /// <param name="digest">The digest to use.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="nc">The nc.</param>
        /// <param name="qop">The qop.</param>
        /// <param name="cnonce">The cnonce.</param>
        /// <param name="digestUri">The digest URI.</param>
        /// <returns>The digest</returns>
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
