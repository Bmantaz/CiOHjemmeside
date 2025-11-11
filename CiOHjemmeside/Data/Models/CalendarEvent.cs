using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    [Table("CalendarEvents")]
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

        // Valgfri navigationsegenskab.
        // Dapper ignorerer denne, medmindre vi laver en specifik JOIN.
        [NotMapped]
        public User? CreatedByUser { get; set; }
    }
}