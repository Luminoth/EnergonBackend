using System;
using System.Text;

namespace EnergonSoftware.Core.Util.Crypt
{
    [Obsolete]
    public class MD5 : Digest
    {
        public override byte[] Hash(string value)
        {
            using(System.Security.Cryptography.MD5 hasher = System.Security.Cryptography.MD5.Create()) {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}
