using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        private readonly HttpListener _listener = new HttpListener();

        private volatile bool _running = false;
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
            }
        }
#endregion

        public void Start(List<string> prefixes)
        {
            Logger.Debug("Starting HttpServer...");

            prefixes.ForEach(prefix => _listener.Prefixes.Add(prefix));

            _listener.Start();
            _task = Task.Run(() => Run());
        }

        public void Stop()
        {
            if(!_running) {
                return;
            }

            Logger.Debug("Stopping HttpServer...");

            _listener.Stop();
            _task.Wait();
            _task = null;
        }

        private void Run()
        {
            RunAsync().Wait();
        }

        private async Task RunAsync()
        {
            try {
                _running = true;
                while(_listener.IsListening) {
                    HttpListenerContext context = await _listener.GetContextAsync().ConfigureAwait(false);
                    await HandleRequestAsync(context.Request, context.Response).ConfigureAwait(false);
                }
            } catch(HttpListenerException e) {
                if(995 != e.ErrorCode) {
                    Logger.Error("Unhandled Exception!", e);
                }
            } catch(Exception e) {
                Logger.Fatal("Unhandled Exception!", e);
            } finally {
                _running = false;
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
string data = "<html><head><title>HttpServer Test</title></head><body>Hello World!</body></html>";
await Task.Delay(1).ConfigureAwait(false);
                return encoding.GetBytes(data);
            }
            return null;
        }
    }
}
