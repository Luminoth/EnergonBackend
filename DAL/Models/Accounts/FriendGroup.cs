namespace EnergonSoftware.DAL.Models.Accounts
{
    public class FriendGroup
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string GroupName { get; set; }
        public int? ParentGroupId { get; set; }

        public virtual AccountInfo Account { get; set; }
        public virtual FriendGroup ParentGroup { get; set; }

        public override string ToString()
        {
            return "FriendGroup(Id: " + Id + ", AccountId: " + AccountId + ", GroupName: " + GroupName + ", ParentGroupId: " + ParentGroupId + ")";
        }
    }
}
