using System;

namespace SocietyMS.Models
{
    /// <summary>Represents a society event.</summary>
    public class Event
    {
        public int EventID { get; set; }
        public int SocietyID { get; set; }
        public string SocietyName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public int MaxAttendees { get; set; }
        public int RegisteredCount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsUpcoming { get { return EventDate > DateTime.Now; } }
        public int SpotsLeft { get { return MaxAttendees - RegisteredCount; } }
        public override string ToString() { return Title; }
    }
}

