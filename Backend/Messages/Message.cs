using System.IO;
using System.Threading.Tasks;
using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Message : IFormattable
    {
        /// <summary>
        /// The meta content type
        /// </summary>
        public const string MetaContentType = "message";

        /// <summary>
        /// Determines whether the contentType is the message contentType.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <returns>True if the contentType is the message contentType</returns>
        public static bool IsMessageContentType(string contentType)
        {
            return contentType.StartsWith(MetaContentType + "/");
        }

        /// <summary>
        /// Gets the type of the message from the contentType.
        /// </summary>
        /// <param name="contentType">Type content type.</param>
        /// <returns>The message type</returns>
        public static string GetMessageType(string contentType)
        {
            if(!IsMessageContentType(contentType)) {
                return null;
            }

            int idx = contentType.IndexOf("/", System.StringComparison.InvariantCultureIgnoreCase);
            return contentType.Substring(idx + 1);
        }

        /// <summary>
        /// Serializes a message using a given formatter.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="formatter">The formatter to use.</param>
        /// <remarks>
        /// This method should be used to serialize messages rather than
        /// calling message.SerializeAsync() directly. This correctly
        /// begins and finishes the message document and flushes the formatter.
        /// </remarks>
        public static async Task SerializeAsync(Message message, IFormatter formatter)
        {
            await formatter.BeginAsync("Message").ConfigureAwait(false);
            await message.SerializeAsync(formatter).ConfigureAwait(false);
            await formatter.FinishAsync().ConfigureAwait(false);
            await formatter.FlushAsync().ConfigureAwait(false);
        }

        public static async Task<Message> DeSerializeAsync(string contentType, string formatterType, byte[] buffer, int offset, int count, IMessageFactory factory)
        {
            if(!IsMessageContentType(contentType)) {
                return null;
            }

            Message message = factory.Create(GetMessageType(contentType));
            IFormatter formatter = new FormatterFactory().Create(formatterType);

            using(MemoryStream messageStream = new MemoryStream()) {
                await messageStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                messageStream.Flip();

                formatter.Attach(messageStream);
                await message.DeserializeAsync(formatter).ConfigureAwait(false);
                return message;
            }
        }

        public string ContentType => MetaContentType + "/" + Type;

        public abstract string Type { get; }

        public abstract Task SerializeAsync(IFormatter formatter);

        public abstract Task DeserializeAsync(IFormatter formatter);
    }
}
