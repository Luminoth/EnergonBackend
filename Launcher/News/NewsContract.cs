using System.Runtime.Serialization;

namespace EnergonSoftware.Launcher.News
{
    [DataContract]
    internal sealed class NewsContract
    {
        [DataMember(Name = "Id")]
        public int Id { get; set; }

        [DataMember(Name = "Date")]
        public string Date { get; set; }

        [DataMember(Name = "Headline")]
        public string Headline { get; set; }

        [DataMember(Name = "NewsUpdate")]
        public /*DateTime*/string NewsUpdate { get; set; }

        public string NewsValue => $"{Headline}\r\n{Date}\r\n\r\n{NewsUpdate}";

        public override string ToString()
        {
            return $"NewsContract(Id={Id}, Date={Date}, Headline={Headline}, NewsUpdate={NewsUpdate})";
        }
    }
}
