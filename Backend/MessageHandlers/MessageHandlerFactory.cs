namespace EnergonSoftware.Backend.MessageHandlers
{
    /// <summary>
    /// Factory for creating message handlers
    /// </summary>
    public interface IMessageHandlerFactory
    {
        /// <summary>
        /// Creates a message handler of the specified type.
        /// </summary>
        /// <param name="type">The message handler type.</param>
        /// <returns>The message handler</returns>
        MessageHandler Create(string type);
    }
}
