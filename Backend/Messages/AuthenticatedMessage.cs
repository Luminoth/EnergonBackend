namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Authenticated message
    /// </summary>
    public abstract class AuthenticatedMessage : Message
    {
        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        public abstract string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public abstract string SessionId { get; set; }
    }
}
