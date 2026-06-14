using System;

namespace SocietyMS.Models
{
    /// <summary>Represents an announcement posted by a society or admin.</summary>
    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public int? SocietyID { get; set; }
        public string SocietyName { get; set; }
        public int PostedBy { get; set; }
        public string PosterName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime CreatedAt { get; set; }
        public override string ToString() { return Title; }
    }
}

