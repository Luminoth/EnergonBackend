﻿using System.ComponentModel.DataAnnotations;

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
        [Required]
        public StartupEventType Type { get; set; } = StartupEventType.Invalid;

        [Required, MaxLength(32)]
        public string Application { get; set; }

        public override string ToString()
        {
            return $"StartupEvent(Id: {Id}, Timestamp: {Timestamp}, Type: {Type}, Application: {Application})";
        }
    }
}
