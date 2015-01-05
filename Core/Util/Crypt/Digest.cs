using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Digest : Hasher
    {
        public async Task<string> DigestPasswordAsync(string username, string realm, string password)
        {
            return await HashHexAsync(username + ":" + realm + ":" + password).ConfigureAwait(false);
        }
    }
}
