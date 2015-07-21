namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Notification
    /// </summary>
    public abstract class NotificationMessage : Message
    {
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public abstract string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        /// <value>
        /// The service identifier.
        /// </value>
        public abstract string ServiceId { get; set; }
    }
}
