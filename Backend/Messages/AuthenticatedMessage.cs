namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Authenticated message
    /// </summary>
    public interface IAuthenticatedMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        string SessionId { get; set; }
    }
}
