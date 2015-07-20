using System.ComponentModel.DataAnnotations;

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
        [Required]
        public AuthenticateEventType Type { get; set; } = AuthenticateEventType.Invalid;

        [Required, MaxLength(32)]
        public string Origin { get; set; }

        [MaxLength(256)]
        public string AccountName { get; set; }

        [MaxLength(256)]
        public string Reason { get; set; }

        public override string ToString()
        {
            return "AuthenticateEvent(Id: " + Id + ", Timestamp: " + Timestamp + ", Type: " + Type + ", Origin: " + Origin + ", AccountName: " + AccountName + ", Reason: " + Reason + ")";
        }
    }
}
