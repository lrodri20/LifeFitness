namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Threading.Tasks;

    public class SearchService : ISearchService
    {
        private readonly SmartFitnessDbContext _context;

        public SearchService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResultsDto> SearchPotentialMatchesAsync(int userId, SearchParameters parameters)
        {
            // Get current user's profile with all related data
            var currentUserProfile = await _context.Profiles
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

            // Get user's preferences or use defaults
            var preferences = currentUserProfile.MatchingPreference ?? new MatchingPreference
            {
                MaxDistanceMiles = 5,
                GenderPreference = GenderPreference.Any,
                PreferSimilarFitnessLevel = true,
                FitnessLevelTolerance = 1
            };

            // Use provided radius or user's preference
            var searchRadius = parameters.Radius ?? preferences.MaxDistanceMiles;

            // Get all matches to track previous interactions
            var allMatches = await _context.MatchRequests
                .Where(m => m.RequesterId == currentUserProfile.Id || m.RequesteeId == currentUserProfile.Id)
                .ToDictionaryAsync(m =>
                    m.RequesterId == currentUserProfile.Id ? m.RequesteeId : m.RequesterId,
                    m => m);

            // Get profile IDs to exclude (active matches and blocked users)
            var excludedProfileIds = allMatches
                .Where(kvp => kvp.Value.Status == MatchStatus.Accepted ||
                             kvp.Value.Status == MatchStatus.Pending ||
                             kvp.Value.Status == MatchStatus.Blocked)
                .Select(kvp => kvp.Key)
                .ToList();

            // Build query for potential matches
            var query = _context.Profiles
                .Include(p => p.User)
                .Include(p => p.Activities)
                    .ThenInclude(pa => pa.Activity)
                .Include(p => p.Goals)
                .Include(p => p.Schedules)
                .Where(p => p.Id != currentUserProfile.Id)
                .Where(p => !excludedProfileIds.Contains(p.Id))
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue);

            // Apply activity filter if specified
            if (!string.IsNullOrWhiteSpace(parameters.ActivityFilter))
            {
                query = query.Where(p => p.Activities.Any(a =>
                    a.Activity.Name.ToLower() == parameters.ActivityFilter.ToLower()));
            }

            // Apply fitness level filter if specified
            if (parameters.FitnessLevelFilter.HasValue)
            {
                query = query.Where(p => p.FitnessLevel == parameters.FitnessLevelFilter.Value);
            }

            // Load all potential profiles
            var potentialProfiles = await query.ToListAsync();

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
                .Where(p => p.Distance <= searchRadius)
                .ToList();

            var today = DateTime.Today;
            profilesWithDistance = profilesWithDistance
    .Where(p =>
    {
        var age = CalculateAge(p.Profile.DateOfBirth);
        return age >= preferences.MinAge && age <= preferences.MaxAge;
    })
    .ToList();



            // Calculate compatibility and create results
            var results = new List<PotentialMatchDto>();

            foreach (var item in profilesWithDistance)
            {
                var profile = item.Profile;
                var compatibilityBreakdown = CalculateCompatibilityBreakdown(
                    currentUserProfile, profile, item.Distance, preferences);

                // Get previous match info if exists
                allMatches.TryGetValue(profile.Id, out var previousMatch);

                results.Add(new PotentialMatchDto
                {
                    UserId = profile.UserId,
                    ProfileId = profile.Id,
                    DisplayName = profile.DisplayName ?? $"{profile.FirstName} {profile.LastName?[0]}.",
                    Age = CalculateAge(profile.DateOfBirth),
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    Bio = profile.Bio,
                    Distance = Math.Round(item.Distance, 1),
                    CompatibilityScore = compatibilityBreakdown.TotalScore,
                    FitnessLevel = profile.FitnessLevel,
                    HasHomeGym = profile.HasHomeGym,
                    Activities = profile.Activities.Select(a => a.Activity.Name).ToList(),
                    Goals = profile.Goals.Select(g => g.Goal.ToString()).ToList(),
                    CommonActivities = GetCommonActivities(currentUserProfile, profile),
                    PreviousMatchId = previousMatch?.Id,
                    PreviousMatchStatus = previousMatch?.Status.ToString(),
                    TypicalAvailability = GetAvailabilityPreview(profile.Schedules),
                    City = profile.City ?? "Unknown",
                    State = profile.State ?? "",
                    CompatibilityBreakdown = compatibilityBreakdown
                });
            }

            // Sort by compatibility score and apply limit
            var sortedResults = results
                .OrderByDescending(r => r.CompatibilityScore)
                .Take(parameters.Limit)
                .ToList();

            // Calculate metadata
            var metadata = new SearchMetadataDto
            {
                SearchRadiusMiles = searchRadius,
                SearchedAt = DateTime.UtcNow,
                ResultsByDistance = CalculateResultsByDistance(results),
                AppliedFilters = GetAppliedFilters(parameters)
            };

            // Add suggestions if few results
            if (sortedResults.Count < 5)
            {
                metadata.SuggestedAction = "Try increasing your search radius or adjusting your preferences to find more matches.";
            }

            return new SearchResultsDto
            {
                Results = sortedResults,
                TotalFound = results.Count,
                ReturnedCount = sortedResults.Count,
                Metadata = metadata
            };
        }

        public async Task<SearchPreviewDto> GetSearchPreviewAsync(int userId)
        {
            var userProfile = await _context.Profiles
                .Include(p => p.MatchingPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null || !userProfile.Latitude.HasValue || !userProfile.Longitude.HasValue)
            {
                throw new InvalidOperationException("Profile or location not set.");
            }

            var radiusOptions = new[] { 5, 10, 15, 25, 50 };
            var matchesByRadius = new Dictionary<int, int>();

            // Get all valid profiles
            var validProfiles = await _context.Profiles
                .Where(p => p.Id != userProfile.Id && p.Latitude.HasValue && p.Longitude.HasValue)
                .Select(p => new { p.Id, p.Latitude, p.Longitude })
                .ToListAsync();

            // Count matches at each radius
            foreach (var radius in radiusOptions)
            {
                var count = validProfiles.Count(p =>
                {
                    var distance = CalculateDistance(
                        userProfile.Latitude!.Value,
                        userProfile.Longitude!.Value,
                        p.Latitude!.Value,
                        p.Longitude!.Value
                    );
                    return distance <= radius;
                });
                matchesByRadius[radius] = count;
            }

            var currentRadius = userProfile.MatchingPreference?.MaxDistanceMiles ?? 5;
            var currentCount = matchesByRadius.ContainsKey(currentRadius)
                ? matchesByRadius[currentRadius]
                : matchesByRadius[5];

            string recommendation;
            if (currentCount < 10)
                recommendation = "Consider increasing your search radius to find more potential matches.";
            else if (currentCount > 50)
                recommendation = "You have many potential matches! Consider adding filters to find the best fits.";
            else
                recommendation = "You have a good number of potential matches in your area.";

            return new SearchPreviewDto
            {
                MatchesByRadius = matchesByRadius,
                CurrentRadius = currentRadius,
                Recommendation = recommendation
            };
        }

        // Helper Methods

        private CompatibilityBreakdownDto CalculateCompatibilityBreakdown(
            Profile currentUser, Profile potentialMatch, double distance, MatchingPreference preferences)
        {
            var breakdown = new CompatibilityBreakdownDto();

            // Distance score (max 40 points)
            if (distance <= 2) breakdown.DistanceScore = 40;
            else if (distance <= 5) breakdown.DistanceScore = 30;
            else if (distance <= 10) breakdown.DistanceScore = 20;
            else breakdown.DistanceScore = 10;

            // Activity overlap score (max 30 points)
            var commonActivities = GetCommonActivities(currentUser, potentialMatch);
            if (currentUser.Activities.Any())
            {
                var overlapPercentage = (double)commonActivities.Count / currentUser.Activities.Count;
                breakdown.ActivityScore = Math.Round(overlapPercentage * 30, 1);
            }

            // Schedule compatibility (max 20 points)
            breakdown.ScheduleScore = Math.Round(CalculateScheduleOverlap(currentUser, potentialMatch) * 20, 1);

            // Fitness level compatibility (max 10 points)
            var levelDiff = Math.Abs((int)currentUser.FitnessLevel - (int)potentialMatch.FitnessLevel);
            if (levelDiff == 0) breakdown.FitnessLevelScore = 10;
            else if (levelDiff == 1) breakdown.FitnessLevelScore = 7;
            else if (levelDiff == 2) breakdown.FitnessLevelScore = 3;
            else breakdown.FitnessLevelScore = 0;

            // Goal alignment bonus (up to 10 bonus points)
            var commonGoals = currentUser.Goals.Select(g => g.Goal)
                .Intersect(potentialMatch.Goals.Select(g => g.Goal))
                .Count();
            breakdown.GoalScore = Math.Min(commonGoals * 3, 10);

            // Calculate total
            breakdown.TotalScore = Math.Round(
                breakdown.DistanceScore +
                breakdown.ActivityScore +
                breakdown.ScheduleScore +
                breakdown.FitnessLevelScore +
                breakdown.GoalScore, 1);

            return breakdown;
        }

        private List<string> GetCommonActivities(Profile profile1, Profile profile2)
        {
            var activities1 = profile1.Activities.Select(a => a.Activity.Name).ToHashSet();
            var activities2 = profile2.Activities.Select(a => a.Activity.Name).ToHashSet();
            return activities1.Intersect(activities2).ToList();
        }

        private double CalculateScheduleOverlap(Profile profile1, Profile profile2)
        {
            if (!profile1.Schedules.Any() || !profile2.Schedules.Any())
                return 0.5; // Default to 50% if no schedule data

            var schedule1 = profile1.Schedules.Where(s => s.IsAvailable).ToList();
            var schedule2 = profile2.Schedules.Where(s => s.IsAvailable).ToList();

            var commonSlots = schedule1
                .Where(s1 => schedule2.Any(s2 => s2.DayOfWeek == s1.DayOfWeek && s2.TimeSlot == s1.TimeSlot))
                .Count();

            var totalSlots = schedule1.Count;
            return totalSlots > 0 ? (double)commonSlots / totalSlots : 0;
        }

        private List<string> GetAvailabilityPreview(ICollection<ProfileSchedule> schedules)
        {
            var preview = new List<string>();

            if (!schedules.Any(s => s.IsAvailable))
                return new List<string> { "No schedule set" };

            // Group by time slot to show most common availability
            var topSlots = schedules
                .Where(s => s.IsAvailable)
                .GroupBy(s => s.TimeSlot)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => GetTimeSlotDisplay(g.Key));

            return topSlots.ToList();
        }

        private string GetTimeSlotDisplay(TimeSlot timeSlot)
        {
            return timeSlot switch
            {
                TimeSlot.EarlyMorning => "Early morning (5-7 AM)",
                TimeSlot.Morning => "Morning (7-9 AM)",
                TimeSlot.MidMorning => "Mid-morning (9-11 AM)",
                TimeSlot.Lunch => "Lunch (11 AM-1 PM)",
                TimeSlot.Afternoon => "Afternoon (1-4 PM)",
                TimeSlot.Evening => "Evening (4-7 PM)",
                TimeSlot.Night => "Night (7-10 PM)",
                _ => timeSlot.ToString()
            };
        }

        private Dictionary<string, int> CalculateResultsByDistance(List<PotentialMatchDto> results)
        {
            return new Dictionary<string, int>
            {
                ["0-2 miles"] = results.Count(r => r.Distance <= 2),
                ["2-5 miles"] = results.Count(r => r.Distance > 2 && r.Distance <= 5),
                ["5-10 miles"] = results.Count(r => r.Distance > 5 && r.Distance <= 10),
                ["10+ miles"] = results.Count(r => r.Distance > 10)
            };
        }

        private List<string> GetAppliedFilters(SearchParameters parameters)
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(parameters.ActivityFilter))
                filters.Add($"Activity: {parameters.ActivityFilter}");

            if (parameters.FitnessLevelFilter.HasValue)
                filters.Add($"Fitness Level: {parameters.FitnessLevelFilter}");

            if (parameters.Radius.HasValue)
                filters.Add($"Custom Radius: {parameters.Radius} miles");

            return filters;
        }

        private int CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue) return 0;

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
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
    }
}