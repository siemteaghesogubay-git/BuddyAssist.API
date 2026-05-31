using BuddyAssist.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BuddyAssist.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Mission> Missions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        

}
}
