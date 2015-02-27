using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Launcher.Updater
{
    internal sealed class UpdateManager : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateManager));

        public static readonly UpdateManager Instance = new UpdateManager();

#region Events
        public event EventHandler<UpdateFinishedEventArgs> OnUpdateFinished;
#endregion

        private string _updateStatus = Properties.Resources.UpdatesLabel;
        public string UpdateStatus
        {
            get { return _updateStatus; }

            private set
            {
                _updateStatus = value;
                NotifyPropertyChanged();
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            // TODO: use string resources here
            Logger.Info("Checking for updates...");

            try {
                using(HttpClient client = new HttpClient()) {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["updateHost"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("updates/launcher").ConfigureAwait(false);
await Task.Delay(2000).ConfigureAwait(false);
                    if(response.IsSuccessStatusCode) {
                        List<UpdateContract> updates = new List<UpdateContract>();
                        using(Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<UpdateContract>));
                            updates = (List<UpdateContract>)serializer.ReadObject(stream);
                        }

                        Logger.Debug("Read updates: " + string.Join(",", (object[])updates.ToArray()));

                        //// TODO: check the updates and then update!

                        UpdateStatus = "Up to date!";
                        if(null != OnUpdateFinished) {
                            OnUpdateFinished(
                                this,
                                new UpdateFinishedEventArgs()
                                {
                                    Success = true,
                                });
                        }
                    } else {
                        UpdateStatus = "Error contacting update server: " + response.ReasonPhrase;
                        if(null != OnUpdateFinished) {
                            OnUpdateFinished(
                                this,
                                new UpdateFinishedEventArgs()
                                {
                                    Success = false,
                                });
                        }
                    }
                }
            } catch(Exception e) {
                UpdateStatus = "Error contacting update server: " + e.Message;
                if(null != OnUpdateFinished) {
                    OnUpdateFinished(
                        this,
                        new UpdateFinishedEventArgs()
                        {
                            Success = false,
                        });
                }
            }
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property = null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private UpdateManager()
        {
        }
    }
}
