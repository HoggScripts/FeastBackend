using System;

namespace Feast.Models
{
    public class CalendarEvent
    {
        public string Title { get; set; } // The title or summary of the event
        public DateTime StartTime { get; set; } // The start time of the event in the user's local time
        public DateTime EndTime { get; set; } // The end time of the event in the user's local time
        public string Description { get; set; } // A description or notes for the event
        public string TimeZone { get; set; } // The timezone of the user

        // Constructor to easily create a new CalendarEvent
        public CalendarEvent(string title, DateTime startTime, DateTime endTime, string description, string timeZone)
        {
            Title = title;
            StartTime = startTime;
            EndTime = endTime;
            Description = description;
            TimeZone = timeZone;
        }

        public CalendarEvent() { } // Parameterless constructor for serialization
    }
}