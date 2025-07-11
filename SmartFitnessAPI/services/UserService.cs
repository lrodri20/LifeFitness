// Services/UsersService.cs
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Models;
using SmartFitnessApi.Models.enums;
using System;

namespace SmartFitnessApi.Services
{
    public class UsersService : IUsersService
    {
        private readonly SmartFitnessDbContext _db;

        public UsersService(SmartFitnessDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserQueueDto>> GetUserQueueAsync(
    int currentUserId,
    string sortBy = "Recent"   // "Recent" | "Compatibility" | "Interaction" | "All"
)
        {
            // 1) Load current user’s Profile
            var me = await _db.Profiles
                .AsNoTracking()
                .SingleAsync(p => p.UserId == currentUserId);

            // 2) Load their preferences
            var prefs = await _db.MatchingPreferences
                .AsNoTracking()
                .SingleAsync(mp => mp.ProfileId == me.Id);

            // 3) Build a blacklist of any Profile.Id you've liked
            var likedByMe = await _db.MatchRequests
                .AsNoTracking()
                .Where(r => r.RequesterId == me.Id)
                .Select(r => r.RequesteeId)
                .ToListAsync();

            // 4) ...plus any who have liked you
            var likedMe = await _db.MatchRequests
                .AsNoTracking()
                .Where(r => r.RequesteeId == me.Id)
                .Select(r => r.RequesterId)
                .ToListAsync();

            // 5) Union them into one set
            var blockedProfileIds = likedByMe
                .Union(likedMe)
                .ToHashSet();

            // 6) Pull all other profiles into memory
            var others = await _db.Profiles
                .AsNoTracking()
                .Where(p => p.UserId != currentUserId)
                .ToListAsync();

            // 7) Apply filters in C#
            var filtered = others
                .Where(p =>
                {
                    // Skip if in our blocked set
                    if (blockedProfileIds.Contains(p.Id))
                        return false;

                    // Age filter
                    var age = DateHelpers.CalculateAge(p.DateOfBirth);
                    if (age < prefs.MinAge || age > prefs.MaxAge)
                        return false;

                    // Distance filter
                    if (!me.Latitude.HasValue || !me.Longitude.HasValue
                     || !p.Latitude.HasValue || !p.Longitude.HasValue)
                        return false;

                    var dist = GeoHelpers.DistanceInMiles(
                        me.Latitude.Value, me.Longitude.Value,
                        p.Latitude.Value, p.Longitude.Value);
                    if (dist > prefs.MaxDistanceMiles)
                        return false;

                    // Home-gym filter
                    if (prefs.PreferHomeGym && !p.HasHomeGym)
                        return false;

                    // Fitness-level tolerance
                    if (prefs.PreferSimilarFitnessLevel
                     && Math.Abs(p.FitnessLevel - me.FitnessLevel) > prefs.FitnessLevelTolerance)
                        return false;

                    return true;
                })
                .ToList();

            // 8) Sort
            IEnumerable<Profile> sorted = sortBy switch
            {
                "Compatibility" => filtered
                    .OrderBy(p => Math.Abs(p.FitnessLevel - me.FitnessLevel)),
                "Interaction" => filtered
                    .OrderByDescending(p =>
                        _db.Messages.Count(m =>
                            (m.SenderId == me.Id && m.Match.User2Id == p.Id) ||
                            (m.SenderId == p.Id && m.Match.User2Id == me.Id))),
                "Recent" => filtered
                    .OrderByDescending(p => p.CreatedAt),
                _ => filtered,
            };

            // 9) Project into DTO
            return sorted.Select(p => new UserQueueDto
            {
                Id = p.UserId,
                Name = p.DisplayName,
                AvatarUrl = p.ProfilePictureUrl,
                Age = DateHelpers.CalculateAge(p.DateOfBirth),
                City = p.City,
                FitnessLevel = (int)p.FitnessLevel
            });
        }
    }
    public static class DateHelpers
    {
        public static int CalculateAge(DateTime? dob)
        {
            if (!dob.HasValue)
                return 0;
            var today = DateTime.UtcNow.Date;
            var age = today.Year - dob.Value.Year;
            if (dob.Value.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }
    public static class GeoHelpers
    {
        // Haversine formula
        public static double DistanceInMiles(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 3958.8; // Earth radius in miles
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * (Math.PI / 180);
    }
}
