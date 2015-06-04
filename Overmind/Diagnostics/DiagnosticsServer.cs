using System.Threading.Tasks;

using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind.Diagnostics
{
    internal sealed class DiagnosticsServer : HttpServer
    {
        public DiagnosticsServer()
        {
            RegisterHandler("/", RootHandler);
            RegisterHandler("/diagnostics", DiagnosticsHandler);
        }

        private async Task<HttpServerResult> RootHandler()
        {
            return await DiagnosticsHandler().ConfigureAwait(false);
        }

        private async Task<HttpServerResult> DiagnosticsHandler()
        {
await Task.Delay(0).ConfigureAwait(false);
            HttpServerResult result = new HttpServerResult();
            return result;
        }
    }
}
