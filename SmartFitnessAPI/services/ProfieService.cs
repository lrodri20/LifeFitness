// Services/ProfileService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public class ProfileService : IProfileService
    {
        private readonly SmartFitnessDbContext _context;

        public ProfileService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileMatchDto?> GetProfileAsync(int userId)
        {
            var profile = await _context.Profiles
                .AsNoTracking()
                .Include(p => p.ProfileActivities)
                    .ThenInclude(pa => pa.Activity)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return null;

            // Calculate age
            var age = 0;
            if (profile.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                age = today.Year - profile.DateOfBirth.Value.Year;
                if (profile.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }

            // Map fitness level enum to name
            var fitnessName = profile.FitnessLevel.ToString();

            // Gather activity names
            var activities = profile.ProfileActivities
                .Select(pa => pa.Activity.Name)
                .ToList();

            return new ProfileMatchDto
            {
                UserId = profile.UserId,
                DisplayName = profile.DisplayName ?? string.Empty,
                Age = age,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                City = profile.City,
                State = profile.State,
                Bio = profile.Bio,
                FitnessLevelName = fitnessName,
                Activities = activities
            };
        }
    }
}