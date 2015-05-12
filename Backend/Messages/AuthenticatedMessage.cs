namespace EnergonSoftware.Backend.Messages
{
    public interface IAuthenticatedMessage : IMessage
    {
        string AccountName { get; set; }
        string SessionId { get; set; }
    }
}
