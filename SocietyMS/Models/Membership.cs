using System;

namespace SocietyMS.Models
{
    /// <summary>Represents a student's society membership.</summary>
    public class Membership
    {
        public int MembershipID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int SocietyID { get; set; }
        public string SocietyName { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public override string ToString() { return UserName + " - " + SocietyName + " (" + Status + ")"; }
    }
}

