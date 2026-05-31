namespace BuddyAssist.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public bool IsPaused { get; set; } = false;
        public int TotalPoints { get; set; } = 0;
        public int CompletedMissions { get; set; } = 0;
        public double Rating { get; set; } = 0;
        public string CurrentLevel { get; set; } = "brons";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string? ProfileImage { get; set; } // Base64


    }
}