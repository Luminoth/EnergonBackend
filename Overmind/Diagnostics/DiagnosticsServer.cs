using System.Threading.Tasks;

using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind.Diagnostics
{
    internal sealed class DiagnosticsServer : HttpServer
    {
        public DiagnosticsServer()
        {
            RegisterHandler("/", RootHandler);
        }

        private async Task<HttpServerResult> RootHandler()
        {
            return await ViewResultAsync("index.html").ConfigureAwait(false);
        }
    }
}
