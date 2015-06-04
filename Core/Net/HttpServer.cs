using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class should only be considered temporary. Embedding MVC is coming in ASP.NET 5
    /// and should be used as a replacement for this when available. The MVC design is much
    /// cleaner and has a lot more tools available for rapid development.
    /// </remarks>
    public class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        public delegate Task<HttpServerResult> HttpRequestDelegate();

        private readonly HttpListener _listener = new HttpListener();

        private ConcurrentDictionary<string, HttpRequestDelegate> _handlers = new ConcurrentDictionary<string,HttpRequestDelegate>();

        private CancellationTokenSource _cancellationToken;
        private Task _task;

        public HttpServer()
        {
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                _listener.Close();
                _cancellationToken.Dispose();
            }
        }
#endregion

        public void Start(IReadOnlyCollection<string> prefixes)
        {
            if(null == prefixes) {
                throw new ArgumentNullException("prefixes");
            }

            if(null != _task) {
                throw new InvalidOperationException(Resources.ErrorHttpServerAlreadyRunning);
            }

            Logger.Debug("Starting HttpServer...");

            foreach(string prefix in prefixes) {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();

            _cancellationToken = new CancellationTokenSource();
            _task = Task.Run(
                async () =>
                {
                    while(!_cancellationToken.IsCancellationRequested) {
                        await RunAsync().ConfigureAwait(false);
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                });
        }

        public void Stop()
        {
            if(null == _task || _cancellationToken.IsCancellationRequested) {
                return;
            }

            Logger.Debug("Stopping HttpServer...");

            _listener.Stop();
            _cancellationToken.Cancel();
            _task.Wait();

            _task = null;
            _cancellationToken = null;

            Logger.Debug("HTTP server finished!");
        }

        protected void RegisterHandler(string url, HttpRequestDelegate handler)
        {
            _handlers[url] = handler;
        }

        protected async Task<HttpServerResult> ViewResultAsync(string path)
        {
            HttpServerResult result = new HttpServerResult();

            // TODO: read the view from disk
await Task.Delay(0).ConfigureAwait(false);

            return result;
        }

        protected HttpServerResult JsonResult(object obj)
        {
            HttpServerResult result = new HttpServerResult();

            using(MemoryStream stream = new MemoryStream()) {
                DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
                json.WriteObject(stream, obj);
                result.Result = stream.ToArray();
            }

            return result;
        }

        private async Task RunAsync()
        {
            if(!_listener.IsListening) {
                return;
            }

            try {
                HttpListenerContext context = await _listener.GetContextAsync().ConfigureAwait(false);
                await HandleRequestAsync(context.Request, context.Response).ConfigureAwait(false);
            } catch(HttpListenerException e) {
                // ignore the normal exit error code
                if(995 != e.ErrorCode) {
                    throw;
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            Logger.Debug("New HttpListenerRequest: " + request.Url);

            if(!_handlers.ContainsKey(request.RawUrl)) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            } else {
                HttpServerResult result = await _handlers[request.RawUrl]().ConfigureAwait(false);
                response.ContentEncoding = result.Encoding;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = null == result.Result ? 0 : result.Result.Length;
                if(null != result.Result) {
                    await response.OutputStream.WriteAsync(result.Result, 0, result.Result.Length).ConfigureAwait(false);
                }
            }

            response.OutputStream.Close();
        }
    }
}
