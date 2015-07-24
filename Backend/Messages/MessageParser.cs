using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Utility class for parsing messages from a buffer
    /// </summary>
    public class MessageParser
    {
        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="contentType">Type of the content (the message type).</param>
        /// <param name="formatterType">Type of the message formatter to use (the message encoding).</param>
        /// <param name="content">The content (the message).</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <returns>The parsed message.</returns>
        public async Task<Message> ParseAsync(string contentType, string formatterType, byte[] content, int contentLength)
        {
            Message message = new BackendMessageFactory().Create(Message.GetMessageType(contentType));
            IFormatter formatter = new FormatterFactory().Create(formatterType);

            using(MemoryStream contentStream = new MemoryStream()) {
                await contentStream.WriteAsync(content, 0, contentLength).ConfigureAwait(false);
                contentStream.Flip();

                formatter.Attach(contentStream);
                await message.DeserializeAsync(formatter).ConfigureAwait(false);
            }
            return message;
        }
    }
}
