namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Factory for creating messages
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates a message with the specified message type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>The message</returns>
        Message Create(string messageType);
    }
}
