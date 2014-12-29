namespace EnergonSoftware.Core.Messages
{
    public interface IAuthenticatedMessage : IMessage
    {
        string Username { get; set; }
        string SessionId { get; set; }
    }
}
