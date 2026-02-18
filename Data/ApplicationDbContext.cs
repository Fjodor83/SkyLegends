using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Models;

namespace SkyLegends.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Poster> Posters { get; set; } = null!;
        public DbSet<Video> Videos { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Poster
            builder.Entity<Poster>(e =>
            {
                e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            });

            // Order
            builder.Entity<Order>(e =>
            {
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasIndex(o => o.StripeSessionId).IsUnique();
                e.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem
            builder.Entity<OrderItem>(e =>
            {
                e.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                e.HasOne(i => i.Poster).WithMany().HasForeignKey(i => i.PosterId).OnDelete(DeleteBehavior.Restrict);
            });

            // UserProfile
            builder.Entity<UserProfile>(e =>
            {
                e.HasIndex(u => u.UserId).IsUnique();
            });
        }
    }
}
