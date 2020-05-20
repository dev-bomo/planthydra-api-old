using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Models
{
    public class Db : IdentityDbContext<User>
    {
        public Db(DbContextOptions<Db> options)
        : base(options)
        {

        }

        public DbSet<User> AppUsers { get; set; }

        public DbSet<ScheduledRun> ScheduledRuns { get; set; }
        public DbSet<WateringScheduleItem> WateringSchedules { get; set; }

        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<ExpoPushToken> ExpoPushTokens { get; set; }

        public DbSet<ProductClient> ProductClients { get; set; }
    }
}