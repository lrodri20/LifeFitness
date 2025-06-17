
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
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public DbSet<RevokedToken> RevokedTokens { get; set; } = null!;
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
            modelBuilder.Entity<Profile>()
                .HasIndex(p => p.UserId)
                .IsUnique();
            modelBuilder.Entity<Profile>()
                .Property(p => p.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();
            modelBuilder.Entity<UserRefreshToken>()
              .HasOne(urt => urt.User)
              .WithMany()
              .HasForeignKey(urt => urt.UserId)
              .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RevokedToken>()
              .HasIndex(rt => rt.JwtId)
              .IsUnique();
            base.OnModelCreating(modelBuilder);
        }

    }
}