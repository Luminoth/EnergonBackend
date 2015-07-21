using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergonSoftware.DAL.Models.Events
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"Event(Id: {Id}, Timestamp: {Timestamp})";
        }
    }
}
