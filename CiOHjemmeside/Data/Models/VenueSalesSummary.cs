namespace CiOHjemmeside.Data.Models
{
    /// <summary>
    /// Opsummerer hvor meget merch der er solgt paa et bestemt spillested/show.
    /// Salg uden tilknyttet koncert samles under "Ukendt".
    /// </summary>
    public class VenueSalesSummary
    {
        public int? ConcertId { get; set; }
        public string VenueName { get; set; } = "Ukendt";
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime? EventDate { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Kort label til grafen, fx "Pumpehuset, Koebenhavn".
        /// </summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(City) ? VenueName : $"{VenueName}, {City}";
    }
}
