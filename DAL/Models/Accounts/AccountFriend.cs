using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergonSoftware.DAL.Models.Accounts
{
    public class AccountFriend
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [ForeignKey("FriendAccount")]
        public int FriendAccountId { get; set; }

        [ForeignKey("Group")]
        public int? GroupId { get; set; }

        public virtual AccountInfo Account { get; set; }

        public virtual AccountInfo FriendAccount { get; set; }

        public virtual FriendGroup Group { get; set; }

        public override string ToString()
        {
            return "AccountFriend(Id: " + Id + ", AccountId: " + AccountId + ", FriendAccountId: " + FriendAccountId + ", GroupId: " + GroupId + ")";
        }
    }
}
