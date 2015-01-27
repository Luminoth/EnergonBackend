using System.Runtime.Serialization;

namespace EnergonSoftware.Launcher.Updater
{
    [DataContract]
    internal sealed class UpdateContract
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
            return "UpdateContract(category=" + Category + ", version=" + Version + ", release date=" + ReleaseDate + ", url=" + Url + ")";
        }
    }
}
