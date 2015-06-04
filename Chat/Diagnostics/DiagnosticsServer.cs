using System.Threading.Tasks;

using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.Diagnostics
{
    internal sealed class DiagnosticsServer : HttpServer
    {
        public DiagnosticsServer()
        {
            RegisterHandler("/", RootHandler);
        }

        private async Task<HttpServerResult> RootHandler()
        {
            return await ViewResultAsync().ConfigureAwait(false);
        }
    }
}
