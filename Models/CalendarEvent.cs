using System;

namespace Feast.Models
{
    public class CalendarEvent
    {
        public string Title { get; set; } 
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; } 
        public string Description { get; set; } 
        public string TimeZone { get; set; } 


        public CalendarEvent(string title, DateTime startTime, DateTime endTime, string description, string timeZone)
        {
            Title = title;
            StartTime = startTime;
            EndTime = endTime;
            Description = description;
            TimeZone = timeZone;
        }

        public CalendarEvent() { } 
    }
}