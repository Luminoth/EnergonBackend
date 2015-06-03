using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        private readonly HttpListener _listener = new HttpListener();

        private CancellationTokenSource _cancellationToken;
        private Task _task;

        public string DefaultIndex { get; set; }

        public HttpServer()
        {
            DefaultIndex = "/index.html";
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
                    // TODO: trap exceptions
                    while(!_cancellationToken.IsCancellationRequested) {
                        await RunAsync().ConfigureAwait(false);
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

            Encoding encoding = response.ContentEncoding;
            if(null == encoding) {
                encoding = Encoding.UTF8;
                response.ContentEncoding = encoding;
            }

            byte[] data = await ReadContentAsync(request.RawUrl, encoding).ConfigureAwait(false);
            if(null == data) {
                Logger.Debug("Content not found!");
                response.StatusCode = (int)HttpStatusCode.NotFound;
            } else {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = data.Length;
                await response.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }

            response.OutputStream.Close();
        }

        private async Task<byte[]> ReadContentAsync(string url, Encoding encoding)
        {
            if("/" == url) {
                url = DefaultIndex;
            }

            Logger.Debug("Reading content for url=" + url);
            if(DefaultIndex == url) {
                // TODO: read from disk or whatever
string data = "<html><head><title>HttpServer Test</title></head><body>Hello World!</body></html>";
try {
    await Task.Delay(1, _cancellationToken.Token).ConfigureAwait(false);
} catch(TaskCanceledException) {
    // ignore this
}
                return encoding.GetBytes(data);
            }

            return null;
        }
    }
}
