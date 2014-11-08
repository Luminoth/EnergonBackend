using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class HttpServer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(HttpServer));

        private Thread _thread;
        HttpListener _listener = new HttpListener();

        public HttpServer()
        {
            _thread = new Thread(new ThreadStart(Run));
        }

        public void Start(List<string> prefixes)
        {
            _logger.Info("Starting HttpServer...");

            prefixes.ForEach(prefix => _listener.Prefixes.Add(prefix));

            _listener.Start();
            _thread.Start();
        }

        public void Stop()
        {
            _logger.Info("Stopping HttpServer...");
            _listener.Stop();
            _thread.Join();
        }

        private void Run()
        {
            try {
                while(_listener.IsListening) {
                    HttpListenerContext context = _listener.GetContext();
                    _logger.Debug("New HttpListenerRequest: " + context.Request.RawUrl);

                    Thread.Sleep(0);
                }
            } catch(Exception e) {
                _logger.Fatal("Unhandled Exception!", e);
            }
        }
    }
}
