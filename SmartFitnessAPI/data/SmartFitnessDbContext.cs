
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
            modelBuilder.Entity<PasswordResetToken>(b =>
            {
                b.ToTable("PasswordResetTokens");
                b.HasKey(p => p.Id);

                b.Property(p => p.Token)
                .IsRequired();

                b.Property(p => p.ExpiresAt)
                .IsRequired();

                b.Property(p => p.Used)
                .IsRequired();

                b.HasOne(p => p.User)
                .WithMany()  // or .WithMany(u=>u.PasswordResetTokens)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            base.OnModelCreating(modelBuilder);
        }

    }
}