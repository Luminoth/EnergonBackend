using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Digest : Hasher
    {
        public async Task<string> DigestPasswordAsync(string account_name, string realm, string password)
        {
            return await HashHexAsync(account_name + ":" + realm + ":" + password).ConfigureAwait(false);
        }
    }
}
