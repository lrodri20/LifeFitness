// Services/LikeService.cs
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public class LikeService : ILikeService
    {
        private readonly SmartFitnessDbContext _context;

        public LikeService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<LikeResultDto> CreateLikeAsync(int userId, int targetUserId)
        {
            if (userId == targetUserId)
                throw new InvalidOperationException("Cannot like yourself.");

            // Prevent duplicate
            bool already = await _context.MatchRequests
                .AnyAsync(m => m.RequesterId == userId && m.RequesteeId == targetUserId);
            if (already)
                throw new InvalidOperationException("Like already exists.");

            // Create pending like
            var like = new MatchRequest
            {
                RequesterId = userId,
                RequesteeId = targetUserId,
                CreatedAt = DateTime.UtcNow,
                RespondedAt = null
            };
            _context.MatchRequests.Add(like);
            await _context.SaveChangesAsync();

            // Check for reciprocal like
            bool isMutual = await _context.MatchRequests
                .AnyAsync(m => m.RequesterId == targetUserId && m.RequesteeId == userId);

            if (isMutual)
            {
                // Build MatchDto
                var partnerProfile = await _context.Profiles
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == targetUserId);

                var matchDto = new MatchDtoOld
                {
                    MatchId = targetUserId,
                    MatchedAt = DateTime.UtcNow,
                    CompatibilityScore = 0, // compute if needed
                    Partner = new PartnerDto
                    {
                        UserId = partnerProfile!.UserId,
                        DisplayName = partnerProfile.DisplayName!,
                        Age = partnerProfile.DateOfBirth.HasValue
                            ? DateTime.UtcNow.Year - partnerProfile.DateOfBirth.Value.Year
                            : 0,
                        ProfilePictureUrl = partnerProfile.ProfilePictureUrl,
                        City = partnerProfile.City,
                        State = partnerProfile.State,
                        DistanceMiles = 0,
                        FitnessLevel = (int)partnerProfile.FitnessLevel,
                        Activities = new System.Collections.Generic.List<string>()
                    }
                };

                return new LikeResultDto { IsMatch = true, Match = matchDto };
            }

            // Return pending like info
            var likeDto = new LikeDto
            {
                LikeId = like.Id,
                FromUserId = like.RequesterId,
                ToUserId = like.RequesteeId,
                LikedAt = like.CreatedAt
            };

            return new LikeResultDto { IsMatch = false, Like = likeDto };
        }
        public async Task<IEnumerable<IncomingLikeDto>> GetIncomingLikesAsync(int userId)
        {
            // 1) Get all incoming match‐requests (to me)
            var incoming = await _context.MatchRequests
                .Where(m => m.RequesteeId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // 2) Get all outgoing requests I've sent (so we can spot mutual likes)
            var outgoingProfileIds = await _context.MatchRequests
                .Where(m => m.RequesterId == userId)
                .Select(m => m.RequesteeId)
                .Distinct()
                .ToListAsync();

            // 3) Filter out mutual likes
            var filtered = incoming
                .Where(r => !outgoingProfileIds.Contains(r.RequesterId))
                .ToList();

            // 4) Batch‐fetch profiles for the remaining requesters
            var requesterIds = filtered
                .Select(r => r.RequesterId)
                .Distinct()
                .ToList();

            var profiles = await _context.Profiles
                .AsNoTracking()
                .Where(p => requesterIds.Contains(p.UserId))
                .Include(p => p.ProfileActivities)
                    .ThenInclude(pa => pa.Activity)
                .ToListAsync();

            // 5) Build your DTOs
            var result = new List<IncomingLikeDto>();
            foreach (var req in filtered)
            {
                var p = profiles.First(pr => pr.UserId == req.RequesterId);

                // calculate age
                var age = 0;
                if (p.DateOfBirth.HasValue)
                {
                    age = DateTime.UtcNow.Year - p.DateOfBirth.Value.Year;
                    if (p.DateOfBirth.Value.Date > DateTime.UtcNow.AddYears(-age))
                        age--;
                }

                var partner = new PartnerDto
                {
                    UserId = p.UserId,
                    DisplayName = p.DisplayName,
                    Age = age,
                    ProfilePictureUrl = p.ProfilePictureUrl,
                    City = p.City,
                    State = p.State,
                    DistanceMiles = 0, // TODO: compute via Haversine if desired
                    FitnessLevel = (int)p.FitnessLevel,
                    Activities = p.ProfileActivities
                                           .Select(pa => pa.Activity.Name)
                                           .ToList()
                };

                result.Add(new IncomingLikeDto
                {
                    LikeId = req.Id,
                    FromUserId = req.RequesterId,
                    LikedAt = req.CreatedAt,
                    From = partner
                });
            }

            return result;
        }
    }
}
