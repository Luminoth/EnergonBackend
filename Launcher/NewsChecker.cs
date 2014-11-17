using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Launcher
{
    class NewsChecker
    {
        private class News
        {
            public string NewsUpdate { get; set; }

            public News()
            {
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
                // TODO: don't hardcode these addresses!
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["newsHost"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("news/launcher");
                if(response.IsSuccessStatusCode) {
                    //News news = await response.Content.ReadAsAsync<News>();
ClientState.Instance.News = "Got some news!";
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
