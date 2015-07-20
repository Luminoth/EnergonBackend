using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Serializes a message
    /// </summary>
    public static class MessageSerializer
    {
        /// <summary>
        /// Serializes the message.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="formatter">The formatter to use.</param>
        public static async Task SerializeAsync(IMessage message, IFormatter formatter)
        {
            await formatter.BeginAsync("message").ConfigureAwait(false);
            await message.SerializeAsync(formatter).ConfigureAwait(false);
            await formatter.FinishAsync().ConfigureAwait(false);
            await formatter.FlushAsync().ConfigureAwait(false);
        }
    }
}
