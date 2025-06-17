
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Data
{
    public class SmartFitnessDbContext : DbContext
    {
        public SmartFitnessDbContext(DbContextOptions<SmartFitnessDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        // Add other DbSets for your entities
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // everything without an explicit schema now uses "auth"
            modelBuilder.HasDefaultSchema("auth");
            modelBuilder.Entity<PasswordResetToken>()
              .HasOne(pr => pr.User)
              .WithMany()  // or .WithMany(u=>u.PasswordResetTokens) if you add that nav on User
              .HasForeignKey(pr => pr.UserId)
              .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }

    }
}