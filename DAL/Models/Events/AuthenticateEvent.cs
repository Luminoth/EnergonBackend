namespace EnergonSoftware.DAL.Models.Events
{
    public enum AuthenticateEventType
    {
        Invalid,
        Request,
        Begin,
        Success,
        Failure,
    }

    public class AuthenticateEvent : Event
    {
        public AuthenticateEventType Type { get; set; }
        public string Origin { get; set; }
        public string AccountName { get; set; }
        public string Reason { get; set; }

        public AuthenticateEvent() : base()
        {
            Type = AuthenticateEventType.Invalid;
        }

        public override string ToString()
        {
            return "AuthenticateEvent(Id: " + Id + ", Timestamp: " + Timestamp + ", Type: " + Type + ", Origin: " + Origin + ", AccountName: " + AccountName + ", Reason: " + Reason + ")";
        }
    }
}
