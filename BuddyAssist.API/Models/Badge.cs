namespace BuddyAssist.API.Models
{
    public class Badge
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int RequiredMissions { get; set; }
    }
}