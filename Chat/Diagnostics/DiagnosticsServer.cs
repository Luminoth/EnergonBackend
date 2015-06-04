﻿using System.Threading.Tasks;

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

// TODO: the goal here basically is "create a model object in Diagnostics/Models
// Register a handler for /diagnostics
// fill out the diagnostic object
// return the diagnostic object using JsonResult()
    }
}
