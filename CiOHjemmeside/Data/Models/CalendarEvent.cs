using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    // Rettet fra [Table("CalendarEvents")] til [Table("calendarevents")]
    [Table("calendarevents")]
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        // "Gig", "Practice", "Discord", "Other"
        public string EventType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }

        // Foreign Key
        public int CreatedByUserId { get; set; }

        [NotMapped]
        public User? CreatedByUser { get; set; }
    }
}