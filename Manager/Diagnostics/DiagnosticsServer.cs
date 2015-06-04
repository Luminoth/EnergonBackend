using System.Threading.Tasks;

using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Manager.Diagnostics
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

// TODO: the goal here basically is to create a model object in Diagnostics/Models (that makes use of DataContract/DataMember)
// Register a handler for /diagnostics
// fill out the diagnostic object
// return the diagnostic object using JsonResult()
    }
}
