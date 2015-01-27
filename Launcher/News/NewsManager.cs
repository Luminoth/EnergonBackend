using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Launcher.News
{
    internal sealed class NewsManager
    {
        private const int MinCheckTimeSeconds = 1000 * 60;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewsManager));

        public static readonly NewsManager Instance = new NewsManager();

        private long _lastCheckTime = 0;

        public async Task UpdateNewsAsync()
        {
            if(Time.CurrentTimeMs < (_lastCheckTime + MinCheckTimeSeconds)) {
                return;
            }
            _lastCheckTime = Time.CurrentTimeMs;

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
                            ClientState.Instance.News = "No news updates!";
                        } else {
                            ClientState.Instance.News = news[0].NewsUpdate;
                        }
                    } else {
                        ClientState.Instance.News = "Error contacting news server: " + response.ReasonPhrase;
                    }
                }
            } catch(Exception e) {
                ClientState.Instance.News = "Error contacting news server: " + e.Message;
            }
        }

        private NewsManager()
        {
        }
    }
}
