using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Launcher
{
    class UpdateChecker : INotifyPropertyChanged
    {
        [DataContract]
        private class Update
        {
            [DataMember(Name="category")]
            public string Category { get; set; }

            [DataMember(Name="version")]
            public string Version { get; set; }

            [DataMember(Name="release_date")]
            public /*DateTime*/string ReleaseDate { get; set; }

            [DataMember(Name="url")]
            public string Url { get; set; }

            public override string ToString()
            {
                return "Update(category=" + Category + ", version=" + Version + ", release date=" + ReleaseDate + ", url=" + Url + ")";
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
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["updateHost"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("updates/launcher");
                if(response.IsSuccessStatusCode) {
                    List<Update> updates = new List<Update>();
                    using(Stream stream = await response.Content.ReadAsStreamAsync()) {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Update>));
                        updates = (List<Update>)serializer.ReadObject(stream);
                    }
                    _logger.Debug("Read updates: " + string.Join(",", (object[])updates.ToArray()));

                    // TODO: check the updates and then update!

                    UpdateStatus = "Up to date!";
                    UpdateFailed = false;
                    Updated = true;

                    await Task.Delay(3000);
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
