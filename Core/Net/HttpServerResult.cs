using System;
using System.Text;

namespace EnergonSoftware.Core.Net
{
    /// <summary>
    /// An HTTP server result.
    /// </summary>
    [Obsolete]
    public class HttpServerResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether cross origins are allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cross origins are allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCrossOrigin { get; set; } = false;

        /// <summary>
        /// Gets or sets the content encoding.
        /// </summary>
        /// <value>
        /// The content encoding.
        /// </value>
        public Encoding ContentEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets the length of the content in bytes.
        /// </summary>
        /// <value>
        /// The length of the content in bytes.
        /// </value>
        public int ContentLength => Content?.Length ?? 0;
    }
}
