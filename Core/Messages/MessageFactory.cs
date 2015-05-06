namespace EnergonSoftware.Core.Messages
{
    public interface IMessageFactory
    {
        IMessage Create(string messageType);
    }
}
