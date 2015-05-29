using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergonSoftware.DAL.Models.Accounts
{
    public class FriendGroup
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [Required, MaxLength(64)]
        public string GroupName { get; set; }

        [ForeignKey("ParentGroup")]
        public int? ParentGroupId { get; set; }

        public virtual AccountInfo Account { get; set; }
        public virtual FriendGroup ParentGroup { get; set; }

        public override string ToString()
        {
            return "FriendGroup(Id: " + Id + ", AccountId: " + AccountId + ", GroupName: " + GroupName + ", ParentGroupId: " + ParentGroupId + ")";
        }
    }
}
