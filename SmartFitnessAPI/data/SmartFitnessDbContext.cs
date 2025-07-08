
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;
using SmartFitnessApi.Models.enums;

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
        public DbSet<Activity> Activities { get; set; }
        public DbSet<MatchRequest> MatchRequests { get; set; }
        public DbSet<ProfileSchedule> ProfileSchedules { get; set; }
        public DbSet<MatchingPreference> MatchingPreferences { get; set; }
        public DbSet<ProfileActivity> ProfileActivities { get; set; } = null!;
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
            modelBuilder.Entity<MatchRequest>()
                .HasOne(m => m.Requester)
                .WithMany()
                .HasForeignKey(m => m.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchRequest>()
                .HasOne(m => m.Requestee)
                .WithMany()
                .HasForeignKey(m => m.RequesteeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProfileActivity>(entity =>
               {
                   entity.ToTable("ProfileActivity");
                   entity.HasKey(pa => pa.Id);
                   entity.Property(pa => pa.Id)
                         .ValueGeneratedOnAdd();

                   entity.Property(pa => pa.ProfileId)
                         .IsRequired();

                   entity.Property(pa => pa.ActivityId)
                         .IsRequired();

                   entity.Property(pa => pa.IsPrimary)
                         .IsRequired();

                   entity.HasOne(pa => pa.Profile)
                         .WithMany(p => p.ProfileActivities)
                         .HasForeignKey(pa => pa.ProfileId)
                         .OnDelete(DeleteBehavior.Cascade);

                   entity.HasOne(pa => pa.Activity)
                         .WithMany(a => a.ProfileActivities)
                         .HasForeignKey(pa => pa.ActivityId)
                         .OnDelete(DeleteBehavior.Cascade);
               });
            base.OnModelCreating(modelBuilder);
        }
    }
    /// <summary>
    /// Extension methods for SmartFitnessDbContext to filter by user preferences and retrieve shared activities.
    /// </summary>
    public static class SmartFitnessDbContextExtensions
    {
        /// <summary>
        /// Filters profiles based on the current user's MatchingPreference and returns candidate DTOs.
        /// </summary>
        public static async Task<List<CandidateDto>> FindByPreferencesAsync(
            this SmartFitnessDbContext context,
            int userId)
        {
            // Load current user's profile
            var me = await context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (me == null || !me.Latitude.HasValue || !me.Longitude.HasValue)
                return new List<CandidateDto>();

            // Load matching preferences
            var prefs = await context.MatchingPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.ProfileId == me.Id);
            if (prefs == null)
                return new List<CandidateDto>();

            var today = DateTime.UtcNow.Date;
            var maxDob = today.AddYears(-prefs.MinAge);
            var minDob = today.AddYears(-prefs.MaxAge - 1).AddDays(1);
            // Query other profiles within age range, include profile-activities
            var profiles = await context.Profiles
                .AsNoTracking()
                .Include(p => p.ProfileActivities)
                    .ThenInclude(pa => pa.Activity)
                .Where(p => p.UserId != userId
                    && EF.Functions.DateDiffYear(p.DateOfBirth, today) >= prefs.MinAge
                    && EF.Functions.DateDiffYear(p.DateOfBirth, today) <= prefs.MaxAge)
                .ToListAsync();

            var candidates = new List<CandidateDto>();
            foreach (var p in profiles)
            {
                // Ensure candidate has location
                if (!p.Latitude.HasValue || !p.Longitude.HasValue)
                    continue;

                // Distance filter
                var dist = HaversineDistance(
                    me.Latitude.Value, me.Longitude.Value,
                    p.Latitude.Value, p.Longitude.Value);
                if (dist > prefs.MaxDistanceMiles)
                    continue;

                // Gender preference
                // if (prefs.GenderPreference != GenderPreference.Any)
                // {
                //     if (prefs.GenderPreference == GenderPreference.Same && p.Gender != me.Gender)
                //         continue;
                //     if (prefs.GenderPreference == GenderPreference.Different && p.Gender == me.Gender)
                //         continue;
                // }

                // Fitness level tolerance
                if (prefs.PreferSimilarFitnessLevel
                    && Math.Abs(p.FitnessLevel - me.FitnessLevel) > prefs.FitnessLevelTolerance)
                    continue;

                // Gym type preference
                // if (!((prefs.PreferHomeGym && p.HasHomeGym)
                //     || (prefs.PreferPublicGym && p.HasPublicGym)
                //     || (prefs.PreferOutdoor && p.PrefersOutdoor)))
                //     continue;

                // // Group workout preference
                // if (!prefs.OpenToGroupWorkouts && p.MaxGroupSize > 1)
                //     continue;
                // if (prefs.OpenToGroupWorkouts && p.MaxGroupSize > prefs.MaxGroupSize)
                //     continue;
                int age = today.Year - p.DateOfBirth.Value.Year;
                if (p.DateOfBirth.Value.Date > today.AddYears(-age))
                    age--;
                // Map to CandidateDto
                candidates.Add(new CandidateDto
                {
                    UserId = p.UserId,
                    DisplayName = p.DisplayName,
                    Age = age,
                    ProfilePictureUrl = p.ProfilePictureUrl,
                    City = p.City,
                    State = p.State,
                    DistanceMiles = dist,
                    FitnessLevel = (int)p.FitnessLevel,
                    HasHomeGym = p.HasHomeGym,
                    Activities = p.ProfileActivities
                        .Select(pa => new ActivityDto
                        {
                            Id = pa.Activity.Id,
                            Name = pa.Activity.Name
                        })
                        .ToList()
                });
            }

            return candidates;
        }

        /// <summary>
        /// Retrieves the list of shared activities between two profile IDs via the join table.
        /// </summary>
        public static async Task<List<ActivityDto>> GetSharedActivitiesAsync(
            this SmartFitnessDbContext context,
            int userId,
            int otherUserId)
        {
            var userActivityIds = context.ProfileActivities
                .Where(pa => pa.ProfileId == userId)
                .Select(pa => pa.ActivityId);

            var otherActivityIds = context.ProfileActivities
                .Where(pa => pa.ProfileId == otherUserId)
                .Select(pa => pa.ActivityId);

            var sharedIds = userActivityIds.Intersect(otherActivityIds);

            var sharedActivities = await context.Activities
                .Where(a => sharedIds.Contains(a.Id))
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();

            return sharedActivities;
        }

        /// <summary>
        /// Calculates the great-circle distance between two points in miles.
        /// </summary>
        private static double HaversineDistance(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 3958.8; // Earth radius in miles
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Pow(Math.Sin(dLat / 2), 2)
                  + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                    * Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return R * c;
        }

        private static double ToRad(double deg) => deg * (Math.PI / 180);
    }
}