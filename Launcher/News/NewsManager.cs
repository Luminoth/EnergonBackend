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

namespace EnergonSoftware.Launcher.News
{
    internal sealed class NewsManager : INotifyPropertyChanged
    {
        // ReSharper disable once InconsistentNaming
        private TimeSpan MinCheckTime { get; } = TimeSpan.FromMilliseconds(1000 * 60);

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewsManager));

        public static readonly NewsManager Instance = new NewsManager();

        private DateTime _lastCheckTime = DateTime.MinValue;

        private string _news = "Checking for news updates...";
        public string News
        {
            get { return _news; }

            set
            {
                _news = value;
                NotifyPropertyChanged();
            }
        }

        public async Task UpdateNewsAsync()
        {
            if(DateTime.Now < (_lastCheckTime + MinCheckTime)) {
                return;
            }

            _lastCheckTime = DateTime.Now;

            // TODO: use string resources here
            Logger.Info("Updating news...");

            try {
                using(HttpClient client = new HttpClient()) {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["newsHost"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("Launcher/News").ConfigureAwait(false);
await Task.Delay(1000).ConfigureAwait(false);
                    if(response.IsSuccessStatusCode) {
                        List<NewsContract> news;
                        using(Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<NewsContract>));
                            news = (List<NewsContract>)serializer.ReadObject(stream);
                        }

                        Logger.Debug("Read news: " + string.Join(",", (object[])news.ToArray()));
                        News = news.Count < 1 ? "No news updates!" : news[0].NewsValue;
                    } else {
                        News = "Error contacting news server: " + response.ReasonPhrase;
                    }
                }
            } catch(Exception e) {
                News = "Error contacting news server: " + e.Message;
            }
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
#endregion

        private NewsManager()
        {
        }
    }
}
