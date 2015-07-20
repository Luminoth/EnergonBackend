using System.Threading.Tasks;

namespace EnergonSoftware.Core.Util.Crypt
{
    /// <summary>
    /// Adds password digesting to the hasher class.
    /// </summary>
    public abstract class Digest : Hasher
    {
        /// <summary>
        /// Digests a password.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="realm">The account realm.</param>
        /// <param name="password">The account password.</param>
        /// <returns>The password digest</returns>
        public async Task<string> DigestPasswordAsync(string accountName, string realm, string password)
        {
            return await HashHexAsync(accountName + ":" + realm + ":" + password).ConfigureAwait(false);
        }
    }
}
