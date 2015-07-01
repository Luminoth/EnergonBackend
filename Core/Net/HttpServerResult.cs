using System.Text;

namespace EnergonSoftware.Core.Net
{
    public class HttpServerResult
    {
        public bool AllowCrossOrigin { get; set; }

        public Encoding ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public int ContentLength { get { return null == Content ? 0 : Content.Length; } }

        public HttpServerResult()
        {
            AllowCrossOrigin = false;
            ContentEncoding = Encoding.UTF8;
        }
    }
}
