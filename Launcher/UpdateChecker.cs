using System;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Launcher
{
    class UpdateChecker : INotifyPropertyChanged
    {
        private class Update
        {
            public string Version { get; set; }
            public string URL { get; set; }

            public Update()
            {
            }
        }

#region Singleton
        private static UpdateChecker _instance = new UpdateChecker();
        public static UpdateChecker Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(UpdateChecker));

        private string _updateStatus = Properties.Resources.UpdatesLabel;
        public string UpdateStatus { get { return _updateStatus; } private set { _updateStatus = value; NotifyPropertyChanged(); } }

        private bool _updateFailed;
        public bool UpdateFailed { get { return _updateFailed; } private set { _updateFailed = value; NotifyPropertyChanged(); } }

        private bool _updated;
        public bool Updated { get { return _updated; } private set { _updated = value; NotifyPropertyChanged(); } }

        public async void CheckForUpdates()
        {
            // TODO: use string resources here
            _logger.Info("Checking for updates...");

            using(HttpClient client = new HttpClient()) {
                // TODO: don't hardcode these addresses!
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["updateHost"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("updates/launcher");
                if(response.IsSuccessStatusCode) {
                    //Update update = await response.Content.ReadAsAsync<Update>();
UpdateStatus = "Up to date!";
                    UpdateFailed = false;
                    Updated = true;

                    ClientState.Instance.CurrentPage = ClientState.Page.Login;
                } else {
UpdateStatus = "Error contacting update server: " + response.ReasonPhrase;
                    UpdateFailed = true;
                    Updated = false;
                }
            }
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private UpdateChecker()
        {
        }
    }
}
