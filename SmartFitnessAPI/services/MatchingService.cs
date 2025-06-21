namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class MatchService : IMatchService
    {
        private readonly SmartFitnessDbContext _context;

        public MatchService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<ActiveMatchesResponseDto> GetActiveMatchesAsync(int userId, string sortBy, int limit, int offset)
        {
            // Get user's profile
            var userProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                throw new InvalidOperationException("User profile not found.");
            }

            // Query for active matches only (Status = Accepted)
            var matchesQuery = _context.MatchRequests
                .Include(m => m.Requester)
                    .ThenInclude(p => p.User)
                .Include(m => m.Requester)
                    .ThenInclude(p => p.Activities)
                        .ThenInclude(pa => pa.Activity)
                .Include(m => m.Requester)
                    .ThenInclude(p => p.Goals)
                .Include(m => m.Requester)
                    .ThenInclude(p => p.Schedules)
                .Include(m => m.Requestee)
                    .ThenInclude(p => p.User)
                .Include(m => m.Requestee)
                    .ThenInclude(p => p.Activities)
                        .ThenInclude(pa => pa.Activity)
                .Include(m => m.Requestee)
                    .ThenInclude(p => p.Goals)
                .Include(m => m.Requestee)
                    .ThenInclude(p => p.Schedules)
                .Where(m => (m.RequesterId == userProfile.Id || m.RequesteeId == userProfile.Id)
                         && m.Status == MatchStatus.Accepted);

            // Get total count
            var totalCount = await matchesQuery.CountAsync();

            // Get all matches for statistics
            var allMatches = await matchesQuery.ToListAsync();

            // Calculate statistics
            var statistics = CalculateMatchStatistics(allMatches, userProfile.Id);

            // Apply sorting
            IOrderedQueryable<MatchRequest> orderedQuery = sortBy.ToLower() switch
            {
                "recent" => matchesQuery.OrderByDescending(m => m.RespondedAt ?? m.CreatedAt),
                "compatibility" => matchesQuery.OrderByDescending(m => m.CompatibilityScore),
                "interaction" => matchesQuery.OrderByDescending(m => m.LastInteractionAt ?? m.RespondedAt),
                _ => matchesQuery.OrderByDescending(m => m.RespondedAt ?? m.CreatedAt) // default to date
            };

            // Apply pagination
            var matches = await orderedQuery
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            // Map to DTOs
            var matchDtos = new List<ActiveMatchDto>();
            var now = DateTime.UtcNow;

            foreach (var match in matches)
            {
                var isRequester = match.RequesterId == userProfile.Id;
                var partner = isRequester ? match.Requestee : match.Requester;
                var partnerUser = partner.User;

                // Calculate distance
                var distance = CalculateDistance(userProfile, partner);

                // Parse shared activities
                var sharedActivities = ParseSharedActivities(match.SharedActivitiesJson);

                // Get availability
                var availability = GetAvailability(partner.Schedules);

                // Calculate match duration
                var matchDate = match.RespondedAt ?? match.CreatedAt;
                var matchDuration = GetDurationString(now - matchDate);

                matchDtos.Add(new ActiveMatchDto
                {
                    MatchId = match.Id,
                    MatchedAt = matchDate,
                    LastInteraction = match.LastInteractionAt ?? matchDate,
                    SharedActivities = sharedActivities,
                    CompatibilityScore = match.CompatibilityScore,
                    MatchDuration = matchDuration,
                    IsRecentMatch = (now - matchDate).TotalDays <= 7,
                    WorkoutCount = 0, // Placeholder for future feature
                    Partner = new WorkoutPartnerDto
                    {
                        UserId = partner.UserId,
                        ProfileId = partner.Id,
                        DisplayName = partner.DisplayName ?? $"{partner.FirstName} {partner.LastName?[0]}.",
                        Age = CalculateAge(partner.DateOfBirth),
                        ProfilePictureUrl = partner.ProfilePictureUrl,
                        Bio = partner.Bio,
                        City = partner.City ?? "Unknown",
                        State = partner.State ?? "",
                        Distance = Math.Round(distance, 1),
                        FitnessLevel = partner.FitnessLevel,
                        Activities = partner.Activities.Select(a => a.Activity.Name).ToList(),
                        Goals = partner.Goals.Select(g => g.Goal.ToString()).ToList(),
                        HasHomeGym = partner.HasHomeGym,
                        ContactInfo = new ContactInfoDto
                        {
                            PhoneNumber = partner.PhoneNumber,
                            Email = partnerUser.Email,
                            PreferredContactMethod = "In-app" // Could be stored in preferences
                        },
                        Availability = availability
                    },
                    Interaction = new MatchInteractionDto
                    {
                        InitialMessage = match.InitialMessage ?? "",
                        LastMessageAt = null, // Placeholder for messaging feature
                        MessageCount = 0, // Placeholder
                        LastWorkoutTogether = null, // Placeholder for workout tracking
                        LastWorkoutActivity = null
                    }
                });
            }

            return new ActiveMatchesResponseDto
            {
                Matches = matchDtos,
                TotalCount = totalCount,
                Offset = offset,
                Limit = limit,
                Statistics = statistics
            };
        }

        public async Task RemoveMatchAsync(int matchId, int userId)
        {
            var userProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                throw new InvalidOperationException("User profile not found.");
            }

            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.Id == matchId
                    && (m.RequesterId == userProfile.Id || m.RequesteeId == userProfile.Id)
                    && m.Status == MatchStatus.Accepted);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found or you don't have permission to remove it.");
            }

            // Instead of deleting, update status to Rejected
            match.Status = MatchStatus.Rejected;
            match.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task BlockUserAsync(int matchId, int userId)
        {
            var userProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                throw new InvalidOperationException("User profile not found.");
            }

            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.Id == matchId
                    && (m.RequesterId == userProfile.Id || m.RequesteeId == userProfile.Id));

            if (match == null)
            {
                throw new InvalidOperationException("Match not found.");
            }

            // Update status to Blocked
            match.Status = MatchStatus.Blocked;
            match.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<ActiveMatchDto> GetMatchDetailsAsync(int matchId, int userId)
        {
            var result = await GetActiveMatchesAsync(userId, "date", 1, 0);
            var match = result.Matches.FirstOrDefault(m => m.MatchId == matchId);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found or not accessible.");
            }

            return match;
        }

        public async Task<int> GetActiveMatchCountAsync(int userId)
        {
            var userProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                return 0;
            }

            return await _context.MatchRequests
                .Where(m => (m.RequesterId == userProfile.Id || m.RequesteeId == userProfile.Id)
                         && m.Status == MatchStatus.Accepted)
                .CountAsync();
        }

        public async Task<bool> AreMatchedAsync(int userId1, int userId2)
        {
            var profile1 = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId1);
            var profile2 = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId2);

            if (profile1 == null || profile2 == null)
            {
                return false;
            }

            return await _context.MatchRequests
                .AnyAsync(m =>
                    ((m.RequesterId == profile1.Id && m.RequesteeId == profile2.Id) ||
                     (m.RequesterId == profile2.Id && m.RequesteeId == profile1.Id)) &&
                    m.Status == MatchStatus.Accepted);
        }

        // Helper Methods

        private MatchesStatisticsDto CalculateMatchStatistics(List<MatchRequest> matches, int profileId)
        {
            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddDays(-30);

            var stats = new MatchesStatisticsDto
            {
                TotalActiveMatches = matches.Count,
                NewMatchesThisWeek = matches.Count(m => (m.RespondedAt ?? m.CreatedAt) >= weekAgo),
                NewMatchesThisMonth = matches.Count(m => (m.RespondedAt ?? m.CreatedAt) >= monthAgo),
                AverageCompatibilityScore = matches.Any() ? Math.Round(matches.Average(m => m.CompatibilityScore), 1) : 0
            };

            // Calculate matches by activity
            var activityCounts = new Dictionary<string, int>();
            foreach (var match in matches)
            {
                var activities = ParseSharedActivities(match.SharedActivitiesJson);
                foreach (var activity in activities)
                {
                    if (activityCounts.ContainsKey(activity))
                        activityCounts[activity]++;
                    else
                        activityCounts[activity] = 1;
                }
            }

            stats.MatchesByActivity = activityCounts;
            stats.MostCommonActivity = activityCounts.Any()
                ? activityCounts.OrderByDescending(kvp => kvp.Value).First().Key
                : "None";

            return stats;
        }

        private double CalculateDistance(Profile profile1, Profile profile2)
        {
            if (!profile1.Latitude.HasValue || !profile1.Longitude.HasValue ||
                !profile2.Latitude.HasValue || !profile2.Longitude.HasValue)
            {
                return 0;
            }

            return CalculateDistance(
                profile1.Latitude.Value,
                profile1.Longitude.Value,
                profile2.Latitude.Value,
                profile2.Longitude.Value
            );
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
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

        private List<string> ParseSharedActivities(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private string GetDurationString(TimeSpan duration)
        {
            if (duration.TotalDays < 1)
                return "Today";
            if (duration.TotalDays < 7)
                return $"{(int)duration.TotalDays} day{((int)duration.TotalDays != 1 ? "s" : "")}";
            if (duration.TotalDays < 30)
                return $"{(int)(duration.TotalDays / 7)} week{((int)(duration.TotalDays / 7) != 1 ? "s" : "")}";
            if (duration.TotalDays < 365)
                return $"{(int)(duration.TotalDays / 30)} month{((int)(duration.TotalDays / 30) != 1 ? "s" : "")}";

            return $"{(int)(duration.TotalDays / 365)} year{((int)(duration.TotalDays / 365) != 1 ? "s" : "")}";
        }

        private AvailabilityDto GetAvailability(ICollection<ProfileSchedule> schedules)
        {
            var availability = new AvailabilityDto();
            var now = DateTime.Now;

            // Group by day
            var scheduleByDay = schedules
                .Where(s => s.IsAvailable)
                .GroupBy(s => s.DayOfWeek)
                .OrderBy(g => ((int)g.Key + 6) % 7) // Start from Monday
                .ToList();

            foreach (var dayGroup in scheduleByDay)
            {
                var dayAvailability = new DayAvailabilityDto
                {
                    Day = dayGroup.Key.ToString(),
                    TimeSlots = dayGroup.Select(s => GetTimeSlotDisplay(s.TimeSlot)).OrderBy(t => t).ToList(),
                    IsToday = dayGroup.Key == now.DayOfWeek,
                    IsTomorrow = dayGroup.Key == now.AddDays(1).DayOfWeek
                };
                availability.WeeklySchedule.Add(dayAvailability);
            }

            // Calculate next available time
            availability.NextAvailable = GetNextAvailableTime(schedules, now);

            // Get preferred times (most frequent)
            availability.PreferredTimes = schedules
                .Where(s => s.IsAvailable)
                .GroupBy(s => s.TimeSlot)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => GetTimeSlotDisplay(g.Key))
                .ToList();

            return availability;
        }

        private string GetTimeSlotDisplay(TimeSlot timeSlot)
        {
            return timeSlot switch
            {
                TimeSlot.EarlyMorning => "5-7 AM",
                TimeSlot.Morning => "7-9 AM",
                TimeSlot.MidMorning => "9-11 AM",
                TimeSlot.Lunch => "11 AM-1 PM",
                TimeSlot.Afternoon => "1-4 PM",
                TimeSlot.Evening => "4-7 PM",
                TimeSlot.Night => "7-10 PM",
                _ => timeSlot.ToString()
            };
        }

        private string GetNextAvailableTime(ICollection<ProfileSchedule> schedules, DateTime now)
        {
            var availableSlots = schedules
                .Where(s => s.IsAvailable)
                .OrderBy(s => ((int)s.DayOfWeek + 7 - (int)now.DayOfWeek) % 7)
                .ThenBy(s => s.TimeSlot)
                .ToList();

            if (!availableSlots.Any())
                return "No schedule set";

            // Find next available slot
            foreach (var slot in availableSlots)
            {
                var daysUntil = ((int)slot.DayOfWeek - (int)now.DayOfWeek + 7) % 7;
                if (daysUntil == 0 && IsTimeSlotPassed(slot.TimeSlot, now))
                    continue;

                if (daysUntil == 0)
                    return $"Today {GetTimeSlotDisplay(slot.TimeSlot)}";
                if (daysUntil == 1)
                    return $"Tomorrow {GetTimeSlotDisplay(slot.TimeSlot)}";

                return $"{slot.DayOfWeek} {GetTimeSlotDisplay(slot.TimeSlot)}";
            }

            // If all slots this week have passed, return first slot next week
            var firstSlot = availableSlots.First();
            return $"Next {firstSlot.DayOfWeek} {GetTimeSlotDisplay(firstSlot.TimeSlot)}";
        }

        private bool IsTimeSlotPassed(TimeSlot timeSlot, DateTime now)
        {
            var currentHour = now.Hour;
            return timeSlot switch
            {
                TimeSlot.EarlyMorning => currentHour >= 7,
                TimeSlot.Morning => currentHour >= 9,
                TimeSlot.MidMorning => currentHour >= 11,
                TimeSlot.Lunch => currentHour >= 13,
                TimeSlot.Afternoon => currentHour >= 16,
                TimeSlot.Evening => currentHour >= 19,
                TimeSlot.Night => currentHour >= 22,
                _ => false
            };
        }
    }
}