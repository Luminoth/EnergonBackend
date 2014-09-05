namespace EnergonSoftware.Core.Util.Crypt
{
    public abstract class Digest : Hasher
    {
        public string DigestPassword(string username, string realm, string password)
        {
            return HashHex(username + ":" + realm + ":" + password);
        }
    }
}
