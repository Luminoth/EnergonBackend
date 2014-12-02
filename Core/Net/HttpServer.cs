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

        private HttpListener _listener = new HttpListener();
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

        public /*async Task*/ void Start(List<string> prefixes)
        {
            Logger.Debug("Starting HttpServer...");

            prefixes.ForEach(prefix => _listener.Prefixes.Add(prefix));

            _listener.Start();
_task = Task.Factory.StartNew(() => Run());
            //await Task.Run(() => Run());
        }

        public void Stop()
        {
            Logger.Debug("Stopping HttpServer...");

            _listener.Stop();
_task.Wait();
_task = null;
        }

        private void Run()
        {
            try {
                while(_listener.IsListening) {
                    HttpListenerContext context = _listener.GetContext();
                    HandleRequest(context.Request, context.Response);
                    Thread.Sleep(0);
                }
            } catch(HttpListenerException e) {
                if(995 != e.ErrorCode) {
                    Logger.Error("Unhandled Exception!", e);
                }
            } catch(Exception e) {
                Logger.Fatal("Unhandled Exception!", e);
            }
        }

        private void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            Logger.Debug("New HttpListenerRequest: " + request.Url);

            Encoding encoding = response.ContentEncoding;
            if(null == encoding) {
                encoding = Encoding.UTF8;
                response.ContentEncoding = encoding;
            }

            byte[] data = Task.Factory.StartNew(() => ReadContent(request.RawUrl, encoding)).Result;
            if(null == data) {
                Logger.Debug("Content not found!");
                response.StatusCode = (int)HttpStatusCode.NotFound;
            } else {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = data.Length;
                response.OutputStream.Write(data, 0, data.Length);
            }
            response.OutputStream.Close();
        }

        private byte[] ReadContent(string url, Encoding encoding)
        {
            if("/" == url) {
                url = DefaultIndex;
            }

            Logger.Debug("Reading content for url=" + url);
            if(DefaultIndex == url) {
                string data = "<html><head><title>HttpServer Test</title></head><body>Hello World!</body></html>";
                return encoding.GetBytes(data);
            }
            return null;
        }
    }
}
