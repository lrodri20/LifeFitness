namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MatchingService : IMatchingService
    {
        private readonly SmartFitnessDbContext _dbContext;
        private const double MILES_TO_METERS = 1609.34;

        public MatchingService(SmartFitnessDbContext _db)
        {
            _dbContext = _db;
        }

        public async Task<IEnumerable<ProfileMatchingDto>> GetPotentialMatchesAsync(int userId, int radiusMiles, int limit)
        {
            // Get the current user's profile with all related data
            var currentUserProfile = await _dbContext.Profiles
                .Include(p => p.Activities)
                .ThenInclude(pa => pa.Activity)
                .Include(p => p.Goals)
                .Include(p => p.Schedules)
                .Include(p => p.MatchingPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (currentUserProfile == null)
            {
                throw new InvalidOperationException("User profile not found. Please complete your profile first.");
            }

            if (!currentUserProfile.Latitude.HasValue || !currentUserProfile.Longitude.HasValue)
            {
                throw new InvalidOperationException("Location not set. Please update your location in your profile.");
            }

            // Get user's preferences
            var preferences = currentUserProfile.MatchingPreference ?? new MatchingPreference
            {
                MaxDistanceMiles = radiusMiles,
                GenderPreference = GenderPreference.Any,
                PreferSimilarFitnessLevel = true,
                FitnessLevelTolerance = 1
            };

            // Get existing matches to exclude
            var existingMatchUserIds = await _dbContext.Matches
                .Where(m => m.RequesterId == currentUserProfile.Id || m.RequesteeId == currentUserProfile.Id)
                .Where(m => m.Status != MatchStatus.Rejected && m.Status != MatchStatus.Expired)
                .Select(m => m.RequesterId == currentUserProfile.Id ? m.RequesteeId : m.RequesterId)
                .ToListAsync();

            // Get all potential profiles within radius
            var potentialProfiles = await _dbContext.Profiles
                .Include(p => p.Activities)
                    .ThenInclude(pa => pa.Activity)
                .Include(p => p.Goals)
                .Include(p => p.Schedules)
                .Where(p => p.Id != currentUserProfile.Id)
                .Where(p => !existingMatchUserIds.Contains(p.Id))
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .ToListAsync();

            // Calculate distances and filter by radius
            var profilesWithDistance = potentialProfiles
                .Select(p => new
                {
                    Profile = p,
                    Distance = CalculateDistance(
                        currentUserProfile.Latitude!.Value,
                        currentUserProfile.Longitude!.Value,
                        p.Latitude!.Value,
                        p.Longitude!.Value
                    )
                })
                .Where(p => p.Distance <= radiusMiles)
                .ToList();

            // Apply age filter if specified
            if (preferences.MinAge.HasValue || preferences.MaxAge.HasValue)
            {
                var today = DateTime.Today;
                profilesWithDistance = profilesWithDistance
                    .Where(p => p.Profile.DateOfBirth.HasValue)
                    .Where(p =>
                    {
                        var age = today.Year - p.Profile.DateOfBirth!.Value.Year;
                        if (p.Profile.DateOfBirth.Value.Date > today.AddYears(-age)) age--;

                        return (!preferences.MinAge.HasValue || age >= preferences.MinAge.Value) &&
                               (!preferences.MaxAge.HasValue || age <= preferences.MaxAge.Value);
                    })
                    .ToList();
            }

            // Calculate compatibility scores and create DTOs
            var matchingProfiles = profilesWithDistance
                .Select(p => new ProfileMatchingDto
                {
                    ProfileId = p.Profile.Id,
                    DisplayName = p.Profile.DisplayName ?? $"{p.Profile.FirstName} {p.Profile.LastName?[0]}.",
                    Age = CalculateAge(p.Profile.DateOfBirth),
                    ProfilePictureUrl = p.Profile.ProfilePictureUrl,
                    Bio = p.Profile.Bio,
                    Distance = Math.Round(p.Distance, 1),
                    FitnessLevel = p.Profile.FitnessLevel,
                    HasHomeGym = p.Profile.HasHomeGym,
                    Activities = p.Profile.Activities.Select(a => a.Activity.Name).ToList(),
                    Goals = p.Profile.Goals.Select(g => g.Goal.ToString()).ToList(),
                    CommonActivities = GetCommonActivities(currentUserProfile, p.Profile),
                    CompatibilityScore = CalculateCompatibilityScore(currentUserProfile, p.Profile, p.Distance, preferences)
                })
                .OrderByDescending(p => p.CompatibilityScore)
                .Take(limit)
                .ToList();

            return matchingProfiles;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula to calculate distance between two points
            var R = 3959; // Radius of the Earth in miles
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private int CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return 0;

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }

        private List<string> GetCommonActivities(Profile profile1, Profile profile2)
        {
            var activities1 = profile1.Activities.Select(a => a.Activity.Name).ToHashSet();
            var activities2 = profile2.Activities.Select(a => a.Activity.Name).ToHashSet();
            return activities1.Intersect(activities2).ToList();
        }

        private double CalculateCompatibilityScore(Profile currentUser, Profile potentialMatch, double distance, MatchingPreference preferences)
        {
            double score = 0;

            // Distance score (max 40 points)
            if (distance <= 2) score += 40;
            else if (distance <= 5) score += 30;
            else if (distance <= 10) score += 20;
            else score += 10;

            // Activity overlap score (max 30 points)
            var commonActivities = GetCommonActivities(currentUser, potentialMatch);
            if (currentUser.Activities.Any())
            {
                var overlapPercentage = (double)commonActivities.Count / currentUser.Activities.Count;
                score += overlapPercentage * 30;
            }

            // Schedule compatibility (max 20 points)
            var scheduleOverlap = CalculateScheduleOverlap(currentUser, potentialMatch);
            score += scheduleOverlap * 20;

            // Fitness level compatibility (max 10 points)
            var levelDiff = Math.Abs((int)currentUser.FitnessLevel - (int)potentialMatch.FitnessLevel);
            if (levelDiff == 0) score += 10;
            else if (levelDiff == 1) score += 7;
            else if (levelDiff == 2) score += 3;

            // Goal alignment bonus (up to 10 bonus points)
            var commonGoals = currentUser.Goals.Select(g => g.Goal)
                .Intersect(potentialMatch.Goals.Select(g => g.Goal))
                .Count();
            if (commonGoals > 0)
            {
                score += Math.Min(commonGoals * 3, 10);
            }

            // Home gym preference bonus
            if (preferences.PreferHomeGym && potentialMatch.HasHomeGym)
            {
                score += 5;
            }

            return Math.Round(score, 2);
        }

        private double CalculateScheduleOverlap(Profile profile1, Profile profile2)
        {
            if (!profile1.Schedules.Any() || !profile2.Schedules.Any())
                return 0.5;

            var schedule1 = profile1.Schedules.Where(s => s.IsAvailable).ToList();
            var schedule2 = profile2.Schedules.Where(s => s.IsAvailable).ToList();

            var commonSlots = schedule1
                .Where(s1 => schedule2.Any(s2 => s2.DayOfWeek == s1.DayOfWeek && s2.TimeSlot == s1.TimeSlot))
                .Count();

            var totalSlots = schedule1.Count;
            return totalSlots > 0 ? (double)commonSlots / totalSlots : 0;
        }
    }
}