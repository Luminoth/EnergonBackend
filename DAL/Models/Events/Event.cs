using System;

namespace EnergonSoftware.DAL.Models.Events
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public Event()  
        {
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return "Event(Id: " + Id + ", Timestamp: " + Timestamp + ")";
        }
    }
}
