using System;

namespace SocietyMS.Models
{
    /// <summary>Represents a task assigned to a society member.</summary>
    public class SocietyTask
    {
        public int TaskID { get; set; }
        public int SocietyID { get; set; }
        public string SocietyName { get; set; }
        public int AssignedTo { get; set; }
        public string AssigneeName { get; set; }
        public int AssignedBy { get; set; }
        public string AssignerName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public override string ToString() { return Title; }
    }
}

