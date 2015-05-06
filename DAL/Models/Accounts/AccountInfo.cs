using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.DAL.Models.Accounts
{
    [Table("Accounts")]
    public class AccountInfo
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string AccountName { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public string PasswordMD5 { get; set; }
        public string PasswordSHA512 { get; set; }
        public string EndPoint { get; set; }
        public string SessionId { get; set; }
        public Visibility Visibility { get; set; }
        public string Status { get; set; }

        public virtual ICollection<FriendGroup> FriendGroups { get; set; }

        [InverseProperty("Account")]
        public virtual ICollection<AccountFriend> Friends { get; set; }

        public AccountInfo()
        {
            Visibility = Visibility.Offline;

            FriendGroups = new List<FriendGroup>();
            Friends = new List<AccountFriend>();
        }

        public async Task SetPassword(string realm, string password)
        {
            PasswordMD5 = await new MD5().DigestPasswordAsync(AccountName, realm, password).ConfigureAwait(false);
            PasswordSHA512 = await new SHA512().DigestPasswordAsync(AccountName, realm, password).ConfigureAwait(false);
        }

        public Account ToAccount()
        {
            EndPoint endPoint = null;
            if(null != EndPoint) {
                string[] endPointStr = EndPoint.Split(':');
                if(2 != endPointStr.Length) {
                    throw new FormatException("Invalid EndPoint!");
                }

                endPoint = new IPEndPoint(IPAddress.Parse(endPointStr[0]), Convert.ToInt32(endPointStr[1]));
            } 

            return new Account()
            {
                Id = Id,
                AccountName = AccountName,
                UserName = UserName,
                SessionId = SessionId,
                EndPoint = endPoint,
                Visibility = Visibility,
            };
        }

        public override string ToString()
        {
            return "Account(Id: " + Id + ", IsActive: " + IsActive
                + ", AccountName: " + AccountName + ", EmailAddress: " + EmailAddress + ", UserName: " + UserName
                + ", PasswordMD5: " + PasswordMD5 + ", PasswordSHA512: " + PasswordSHA512
                + ", EndPoint: " + EndPoint + ", SessionId: " + SessionId + ", Visibility: " + Visibility + ", Status: " + Status + ")";
        }
    }
}
