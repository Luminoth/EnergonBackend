namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Notification
    /// </summary>
    public interface INotificationMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        /// <value>
        /// The service identifier.
        /// </value>
        string ServiceId { get; set; }
    }
}
