using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Launcher
{
    internal class NewsChecker
    {
        [DataContract]
        private class News
        {
            [DataMember(Name="category")]
            public string Category { get; set; }

            [DataMember(Name="date")]
            public string Date { get; set; }

            [DataMember(Name="news")]
            public /*DateTime*/string NewsValue { get; set; }

            public string NewsUpdate { get { return Date + "\r\n\r\n" + NewsValue; } }

            public override string ToString()
            {
                return "News(category=" + Category + ", date=" + Date + ", news=" + NewsValue + ")";
            }
        }

        private const int MinCheckTimeSeconds = 1000 * 60;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewsChecker));

        public static readonly NewsChecker Instance = new NewsChecker();

        private long _lastCheckTime = 0;

        public async Task UpdateNewsAsync()
        {
            if(Time.CurrentTimeMs < (_lastCheckTime + MinCheckTimeSeconds)) {
                return;
            }
            _lastCheckTime = Time.CurrentTimeMs;

            // TODO: use string resources here
            Logger.Info("Updating news...");

            using(HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["newsHost"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("news/launcher").ConfigureAwait(false);
                if(response.IsSuccessStatusCode) {
                    List<News> news = new List<News>();
                    using(Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<News>));
                        news = (List<News>)serializer.ReadObject(stream);
                    }
                    Logger.Debug("Read news: " + string.Join(",", (object[])news.ToArray()));

await Task.Delay(2000).ConfigureAwait(false);
                    if(news.Count < 1) {
                        ClientState.Instance.News = "No news updates!";
                    } else {
                        ClientState.Instance.News = news[0].NewsUpdate;
                    }
                } else {
                    ClientState.Instance.News = "Error contacting news server: " + response.ReasonPhrase;
                }
            }
        }

        private NewsChecker()
        {
        }
    }
}
