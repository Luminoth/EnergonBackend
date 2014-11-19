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

using log4net;

namespace EnergonSoftware.Launcher
{
    class NewsChecker
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

#region Singleton
        private static NewsChecker _instance = new NewsChecker();
        public static NewsChecker Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(NewsChecker));

        public async void UpdateNews()
        {
            // TODO: use string resources here
            _logger.Info("Updating news...");

            using(HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["newsHost"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("news/launcher");
                if(response.IsSuccessStatusCode) {
                    List<News> news = new List<News>();
                    using(Stream stream = await response.Content.ReadAsStreamAsync()) {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<News>));
                        news = (List<News>)serializer.ReadObject(stream);
                    }
                    _logger.Debug("Read news: " + string.Join(",", (object[])news.ToArray()));

                    await Task.Delay(3000);
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
