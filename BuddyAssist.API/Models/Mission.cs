namespace BuddyAssist.API.Models
{
    public class Mission
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "open";
        public int Points { get; set; }
        public double DistanceKm { get; set; }
        public int CreatedByUserId { get; set; }
        public int? TakenByUserId { get; set; }
        public int? HelperRating { get; set; }
        public string? HelperComment { get; set; }


        // Sponsrade fält
        public bool IsSponsored { get; set; } = false;
        public string? SponsorName { get; set; }
        public string? SponsorLogo { get; set; }
        public string? SponsorUrl { get; set; }
        public decimal? SponsorBudget { get; set; }
        public int SponsorClicks { get; set; } 
        public DateTime? SponsorExpiresAt { get; set; }



    }
}
