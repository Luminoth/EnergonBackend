﻿using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

using log4net;
using log4net.Config;

using EnergonSoftware.Core;

namespace EnergonSoftware.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(App));

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }
#endregion

#region UI Helpers
        private void OnError(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }
#endregion

#region Event Handlers
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Int32.Parse(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Int32.Parse(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);

            ClientState.Instance.OnDisconnect += OnDisconnectCallback;
            ClientState.Instance.OnSocketError += OnSocketErrorCallback;

            ComponentDispatcher.ThreadIdle += OnIdle;
        }

        private void OnIdle(object sender, EventArgs evt)
        {
            ClientState.Instance.Run();
        }
#endregion

        // TODO: move these into ClientState
        private void OnDisconnectCallback(int socketId)
        {
            if(socketId == ClientState.Instance.AuthSocketId && !ClientState.Instance.Authenticated) {
                OnError("Auth Server Disconnected!", "Disconnected!");
            }
        }

        private void OnSocketErrorCallback(int socketId, string error)
        {
            OnError(error, "Socket Error");
        }
    }
}
