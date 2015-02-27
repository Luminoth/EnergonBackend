using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Digest : Hasher
    {
        public async Task<string> DigestPasswordAsync(string accountName, string realm, string password)
        {
            return await HashHexAsync(accountName + ":" + realm + ":" + password).ConfigureAwait(false);
        }
    }
}
