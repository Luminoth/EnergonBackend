using System.Text;

namespace EnergonSoftware.Core.Util.Crypt
{
    public class SHA512 : Digest
    {
        public override byte[] Hash(string value)
        {
            using(System.Security.Cryptography.SHA512 hasher = System.Security.Cryptography.SHA512.Create()) {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}
