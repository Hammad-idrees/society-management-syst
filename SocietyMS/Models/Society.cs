using System;

namespace SocietyMS.Models
{
    /// <summary>Represents a university student society.</summary>
    public class Society
    {
        public int SocietyID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int? HeadUserID { get; set; }
        public string HeadName { get; set; }
        public string LogoPath { get; set; }
        public string Status { get; set; }
        public int MaxMembers { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public override string ToString() { return Name; }
    }
}

