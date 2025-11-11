namespace CiOHjemmeside.Data.Models
{
    public class Concert
    {
        public int Id { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime EventDate { get; set; }
        public string? TicketLink { get; set; }
        public bool IsSoldOut { get; set; }
    }
}