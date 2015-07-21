using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required, MaxLength(256)]
        public string AccountName { get; set; }

        [Required, MaxLength(64)]
        public string EmailAddress { get; set; }

        [Required, MaxLength(256)]
        public string UserName { get; set; }

        [Required, MaxLength(32)]
        // ReSharper disable once InconsistentNaming
        public string PasswordMD5 { get; set; }

        [Required, MaxLength(128)]
        // ReSharper disable once InconsistentNaming
        public string PasswordSHA512 { get; set; }

        [MaxLength(32)]
        public string EndPoint { get; set; }

        [MaxLength(256)]
        public string SessionId { get; set; }

        [Required]
        public Visibility Visibility { get; set; } = Visibility.Offline;

        [MaxLength(256)]
        public string Status { get; set; }

        public virtual ICollection<FriendGroup> FriendGroups { get; set; } = new List<FriendGroup>();

        [InverseProperty("Account")]
        public virtual ICollection<AccountFriend> Friends { get; set; } = new List<AccountFriend>();

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
                Username = UserName,
                SessionId = SessionId,
                EndPoint = endPoint,
                Visibility = Visibility,
            };
        }

        public override string ToString()
        {
            return $"AccountInfo(Id: {Id}, IsActive: {IsActive}, AccountName: {AccountName}, EmailAddress: {EmailAddress}"
                + $", UserName: {UserName}, PasswordMD5: {PasswordMD5}, PasswordSHA512: {PasswordSHA512}"
                + $", EndPoint: {EndPoint}, SessionId: {SessionId}, Visibility: {Visibility}, Status: {Status})";
        }
    }
}
