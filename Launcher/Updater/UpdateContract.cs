using System.Runtime.Serialization;

namespace EnergonSoftware.Launcher.Updater
{
    [DataContract]
    internal sealed class UpdateContract
    {
        [DataMember(Name = "Id")]
        public int Id { get; set; }

        [DataMember(Name = "Version")]
        public string Version { get; set; }

        [DataMember(Name = "ReleaseDate")]
        public /*DateTime*/string ReleaseDate { get; set; }

        [DataMember(Name = "Url")]
        public string Url { get; set; }

        public override string ToString()
        {
            return "UpdateContract(Id=" + Id + ", Version=" + Version + ", ReleaseDate=" + ReleaseDate + ", Url=" + Url + ")";
        }
    }
}
