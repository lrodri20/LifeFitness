namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Data.Dtos;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Threading.Tasks;

    public class PreferencesService : IPreferencesService
    {
        private readonly SmartFitnessDbContext _context;

        public PreferencesService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<MatchingPreferenceDto> GetMatchingPreferencesAsync(int userId)
        {
            // Get the user's profile
            var profile = await _context.Profiles
                .Include(p => p.MatchingPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile not found. Please complete your profile first.");
            }

            // If no preferences exist, return default preferences
            if (profile.MatchingPreference == null)
            {
                return new MatchingPreferenceDto
                {
                    Id = 0,
                    ProfileId = profile.Id,
                    MaxDistanceMiles = 5,
                    MinAge = 18,
                    MaxAge = 100,
                    GenderPreference = GenderPreference.Any.ToString(),
                    PreferSimilarFitnessLevel = true,
                    FitnessLevelTolerance = 1,
                    PreferHomeGym = false,
                    PreferPublicGym = true,
                    PreferOutdoor = true,
                    OpenToGroupWorkouts = true,
                    MaxGroupSize = 4,
                    LastUpdated = null
                };
            }

            // Map to DTO
            return new MatchingPreferenceDto
            {
                Id = profile.MatchingPreference.Id,
                ProfileId = profile.Id,
                MaxDistanceMiles = profile.MatchingPreference.MaxDistanceMiles,
                MinAge = profile.MatchingPreference.MinAge,
                MaxAge = profile.MatchingPreference.MaxAge,
                GenderPreference = profile.MatchingPreference.GenderPreference.ToString(),
                PreferSimilarFitnessLevel = profile.MatchingPreference.PreferSimilarFitnessLevel,
                FitnessLevelTolerance = profile.MatchingPreference.FitnessLevelTolerance,
                PreferHomeGym = profile.MatchingPreference.PreferHomeGym,
                PreferPublicGym = profile.MatchingPreference.PreferPublicGym,
                PreferOutdoor = profile.MatchingPreference.PreferOutdoor,
                OpenToGroupWorkouts = profile.MatchingPreference.OpenToGroupWorkouts,
                MaxGroupSize = profile.MatchingPreference.MaxGroupSize,
                LastUpdated = profile.UpdatedAt
            };
        }

        public async Task<MatchingPreferenceDto> UpdateMatchingPreferencesAsync(int userId, UpdateMatchingPreferenceDto preferencesDto)
        {
            // Get the user's profile
            var profile = await _context.Profiles
                .Include(p => p.MatchingPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile not found. Please complete your profile first.");
            }

            // Parse gender preference enum
            if (!Enum.TryParse<GenderPreference>(preferencesDto.GenderPreference, out var genderPref))
            {
                throw new InvalidOperationException("Invalid gender preference value.");
            }

            // Update or create preferences
            if (profile.MatchingPreference == null)
            {
                // Create new preferences
                profile.MatchingPreference = new MatchingPreference
                {
                    ProfileId = profile.Id
                };
                _context.MatchingPreferences.Add(profile.MatchingPreference);
            }

            // Update values
            profile.MatchingPreference.MaxDistanceMiles = preferencesDto.MaxDistanceMiles;
            profile.MatchingPreference.MinAge = preferencesDto.MinAge;
            profile.MatchingPreference.MaxAge = preferencesDto.MaxAge;
            profile.MatchingPreference.GenderPreference = genderPref;
            profile.MatchingPreference.PreferSimilarFitnessLevel = preferencesDto.PreferSimilarFitnessLevel;
            profile.MatchingPreference.FitnessLevelTolerance = preferencesDto.FitnessLevelTolerance;
            profile.MatchingPreference.PreferHomeGym = preferencesDto.PreferHomeGym;
            profile.MatchingPreference.PreferPublicGym = preferencesDto.PreferPublicGym;
            profile.MatchingPreference.PreferOutdoor = preferencesDto.PreferOutdoor;
            profile.MatchingPreference.OpenToGroupWorkouts = preferencesDto.OpenToGroupWorkouts;
            profile.MatchingPreference.MaxGroupSize = preferencesDto.MaxGroupSize;

            // Update profile's UpdatedAt timestamp
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated preferences
            return new MatchingPreferenceDto
            {
                Id = profile.MatchingPreference.Id,
                ProfileId = profile.Id,
                MaxDistanceMiles = profile.MatchingPreference.MaxDistanceMiles,
                MinAge = profile.MatchingPreference.MinAge,
                MaxAge = profile.MatchingPreference.MaxAge,
                GenderPreference = profile.MatchingPreference.GenderPreference.ToString(),
                PreferSimilarFitnessLevel = profile.MatchingPreference.PreferSimilarFitnessLevel,
                FitnessLevelTolerance = profile.MatchingPreference.FitnessLevelTolerance,
                PreferHomeGym = profile.MatchingPreference.PreferHomeGym,
                PreferPublicGym = profile.MatchingPreference.PreferPublicGym,
                PreferOutdoor = profile.MatchingPreference.PreferOutdoor,
                OpenToGroupWorkouts = profile.MatchingPreference.OpenToGroupWorkouts,
                MaxGroupSize = profile.MatchingPreference.MaxGroupSize,
                LastUpdated = profile.UpdatedAt
            };
        }

        public async Task DeleteMatchingPreferencesAsync(int userId)
        {
            // Get the user's profile with preferences
            var profile = await _context.Profiles
                .Include(p => p.MatchingPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile not found.");
            }

            if (profile.MatchingPreference == null)
            {
                // No preferences exist, nothing to reset
                return;
            }

            // Reset to default values instead of removing
            profile.MatchingPreference.MaxDistanceMiles = 5;
            profile.MatchingPreference.MinAge = 18;
            profile.MatchingPreference.MaxAge = 100;
            profile.MatchingPreference.GenderPreference = GenderPreference.Any;
            profile.MatchingPreference.PreferSimilarFitnessLevel = true;
            profile.MatchingPreference.FitnessLevelTolerance = 1;
            profile.MatchingPreference.PreferHomeGym = false;
            profile.MatchingPreference.PreferPublicGym = true;
            profile.MatchingPreference.PreferOutdoor = true;
            profile.MatchingPreference.OpenToGroupWorkouts = true;
            profile.MatchingPreference.MaxGroupSize = 4;

            // Update profile timestamp
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}