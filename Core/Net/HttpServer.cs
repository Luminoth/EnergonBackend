using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
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
    [Obsolete("Replace with embedding MVC from ASP.NET 5")]
    public class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        /// <summary>
        /// 
        /// </summary>
        public delegate Task<HttpServerResult> HttpRequestDelegate();

        private readonly HttpListener _listener = new HttpListener();

        private readonly ConcurrentDictionary<string, HttpRequestDelegate> _handlers = new ConcurrentDictionary<string,HttpRequestDelegate>();

        private CancellationTokenSource _cancellationToken;
        private Task _task;

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

        /// <summary>
        /// Starts the server with the specified prefixes.
        /// </summary>
        /// <param name="prefixes">The prefixes.</param>
        /// <exception cref="System.ArgumentNullException">prefixes</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void Start(IReadOnlyCollection<string> prefixes)
        {
            if(null == prefixes) {
                throw new ArgumentNullException(nameof(prefixes));
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

        /// <summary>
        /// Stops the server.
        /// </summary>
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

        /// <summary>
        /// Registers a URL handler.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="handler">The handler.</param>
        protected void RegisterHandler(string url, HttpRequestDelegate handler)
        {
            _handlers[url] = handler;
        }

        /// <summary>
        /// Returns a view result.
        /// </summary>
        /// <param name="path">The view path.</param>
        /// <returns>The result.</returns>
        protected async Task<HttpServerResult> ViewResultAsync(string path)
        {
            HttpServerResult result = new HttpServerResult()
            {
                ContentType = "text/html",
            };

            // TODO: read the view from disk
await Task.Delay(0).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Returns a JSON result.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The result.</returns>
        protected HttpServerResult JsonResult(object obj)
        {
            HttpServerResult result = new HttpServerResult()
            {
                ContentType = "application/json",
            };

            if(null == obj) {
                return result;
            }

            using(MemoryStream stream = new MemoryStream()) {
                DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
                json.WriteObject(stream, obj);

                result.Content = stream.ToArray();
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
            Logger.Debug($"New HttpListenerRequest: {request.Url}");

            if(!_handlers.ContainsKey(request.RawUrl)) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            } else {
                HttpServerResult result = await _handlers[request.RawUrl]().ConfigureAwait(false);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentEncoding = result.ContentEncoding;
                response.ContentType = result.ContentType;
                response.ContentLength64 = result.ContentLength;

                if(result.AllowCrossOrigin) {
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                }

                if(null != result.Content) {
                    await response.OutputStream.WriteAsync(result.Content, 0, result.ContentLength).ConfigureAwait(false);
                }
            }

            response.OutputStream.Close();
        }
    }
}
