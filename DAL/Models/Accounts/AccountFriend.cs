namespace EnergonSoftware.DAL.Models.Accounts
{
    public class AccountFriend
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
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
