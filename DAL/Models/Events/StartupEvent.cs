namespace EnergonSoftware.DAL.Models.Events
{
    public enum StartupEventType
    {
        Invalid,
        Startup,
        Shutdown,
    }

    public class StartupEvent : Event
    {
        public StartupEventType Type { get; set; }
        public string Application { get; set; }

        public StartupEvent() : base()
        {
            Type = StartupEventType.Invalid;
        }

        public override string ToString()
        {
            return "StartupEvent(Id: " + Id + ", Timestamp: " + Timestamp + ", Type: " + Type + ", Application: " + Application + ")";
        }
    }
}
