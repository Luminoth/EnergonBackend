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

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Launcher.News
{
    internal sealed class NewsManager : INotifyPropertyChanged
    {
        private const int MinCheckTimeMS = 1000 * 60;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewsManager));

        public static readonly NewsManager Instance = new NewsManager();

        private long _lastCheckTimeMS = 0;

        private string _news = "Checking for news updates...";
        public string News { get { return _news; } set { _news = value; NotifyPropertyChanged(); } }

        public async Task UpdateNewsAsync()
        {
            if(Time.CurrentTimeMs < (_lastCheckTimeMS + MinCheckTimeMS)) {
                return;
            }
            _lastCheckTimeMS = Time.CurrentTimeMs;

            // TODO: use string resources here
            Logger.Info("Updating news...");

            try {
                using(HttpClient client = new HttpClient()) {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["newsHost"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("news/launcher").ConfigureAwait(false);
await Task.Delay(2000).ConfigureAwait(false);
                    if(response.IsSuccessStatusCode) {
                        List<NewsContract> news = new List<NewsContract>();
                        using(Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<NewsContract>));
                            news = (List<NewsContract>)serializer.ReadObject(stream);
                        }
                        Logger.Debug("Read news: " + string.Join(",", (object[])news.ToArray()));

                        if(news.Count < 1) {
                            News = "No news updates!";
                        } else {
                            News = news[0].NewsUpdate;
                        }
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
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private NewsManager()
        {
        }
    }
}
