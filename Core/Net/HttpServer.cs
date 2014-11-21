using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        private Thread _thread;
        private HttpListener _listener = new HttpListener();

        public string DefaultIndex { get; set; }

        public HttpServer()
        {
            _thread = new Thread(new ThreadStart(Run));

            DefaultIndex = "/index.html";
        }

        public void Dispose()
        {
            _listener.Close();
        }

        public void Start(List<string> prefixes)
        {
            Logger.Debug("Starting HttpServer...");

            prefixes.ForEach(prefix => _listener.Prefixes.Add(prefix));

            _listener.Start();
            _thread.Start();
        }

        public void Stop()
        {
            Logger.Debug("Stopping HttpServer...");

            _listener.Stop();
            _thread.Join();
        }

        private void Run()
        {
            try {
                while(_listener.IsListening) {
                    HttpListenerContext context = _listener.GetContext();
                    HandleRequest(context.Request, context.Response);
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

            byte[] data = ReadContent(request.RawUrl, encoding);
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
