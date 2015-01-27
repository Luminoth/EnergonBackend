using System.Runtime.Serialization;

namespace EnergonSoftware.Launcher.News
{
    [DataContract]
    internal sealed class NewsContract
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
            return "NewsContract(category=" + Category + ", date=" + Date + ", news=" + NewsValue + ")";
        }
    }
}
