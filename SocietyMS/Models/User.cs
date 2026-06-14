using System;

namespace SocietyMS.Models
{
    /// <summary>Represents a system user (Student, SocietyHead, or Admin).</summary>
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string RollNumber { get; set; }
        public string Department { get; set; }
        public int? Semester { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsAdmin { get { return Role == "Admin"; } }
        public bool IsSocietyHead { get { return Role == "SocietyHead"; } }
        public bool IsStudent { get { return Role == "Student"; } }

        public override string ToString() { return FullName + " (" + Role + ")"; }
    }
}

