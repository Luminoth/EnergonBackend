using System;
using System.Diagnostics;
using System.Windows;

using EnergonSoftware.Core;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void InitializeDatabases()
        {
            Trace.WriteLine("Initializing databases...");
        }

        private void ConfigureLogging()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener("DbInit.log", "DbInit"));
            Trace.AutoFlush = true;
        }

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();
        }
#endregion
    }
}
