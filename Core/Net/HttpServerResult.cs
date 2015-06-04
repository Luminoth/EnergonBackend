using System.Text;

namespace EnergonSoftware.Core.Net
{
    public class HttpServerResult
    {
        public Encoding Encoding { get; set; }
        public byte[] Result { get; set; }

        public HttpServerResult()
        {
            Encoding = Encoding.UTF8;
        }
    }
}
