// Services/LikeService.cs
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;
using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Services
{
    public class LikeService : ILikeService
    {
        private readonly SmartFitnessDbContext _context;

        public LikeService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<LikeResultDto> CreateLikeAsync(int me, int them)
        {
            if (me == them)
                throw new ArgumentException("Cannot like yourself");

            using var tx = await _context.Database.BeginTransactionAsync();

            // 1) Prevent duplicate likes
            var exists = await _context.MatchRequests
                .AnyAsync(r => r.RequesterId == me && r.RequesteeId == them);
            if (exists)
                throw new InvalidOperationException("You have already liked this user");

            // 2) Insert your swipe
            var req = new MatchRequest
            {
                RequesterId = me,
                RequesteeId = them,
                CreatedAt = DateTime.UtcNow,
                Status = MatchStatus.Pending
            };
            _context.MatchRequests.Add(req);
            await _context.SaveChangesAsync();

            // 3) Check for reciprocal swipe
            var reciprocal = await _context.MatchRequests
                .SingleOrDefaultAsync(r => r.RequesterId == them && r.RequesteeId == me);

            LikeResultDto result;
            if (reciprocal != null && reciprocal.Status == MatchStatus.Pending)
            {
                // 4a) Mark both as accepted
                reciprocal.Status = req.Status = MatchStatus.Accepted;
                reciprocal.RespondedAt = req.RespondedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 4b) Create the Match record
                var match = new Match
                {
                    User1Id = me,
                    User2Id = them,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Matches.Add(match);
                await _context.SaveChangesAsync();

                // 4c) Manually map to your MatchDto
                result = new LikeResultDto
                {
                    IsMatch = true,
                    Match = new MatchDtoOld
                    {
                        MatchId = match.Id,                        // int or string, whatever your DTO expects
                        MatchedAt = match.CreatedAt,
                        OtherUser = new OtherUserDto
                        {
                            Id = them,
                            Name = (await _context.Profiles
                                           .Where(p => p.UserId == them)
                                           .Select(p => p.DisplayName)
                                           .FirstAsync()),
                            AvatarUrl = (await _context.Profiles
                                           .Where(p => p.UserId == them)
                                           .Select(p => p.ProfilePictureUrl)
                                           .FirstAsync())
                        }
                    }
                };
            }
            else
            {
                // 5) Just return a LikeDto
                result = new LikeResultDto
                {
                    IsMatch = false,
                    Like = new LikeDto
                    {
                        LikeId = req.Id,
                        FromUserId = me,
                        ToUserId = them,
                        LikedAt = req.CreatedAt
                    }
                };
            }

            await tx.CommitAsync();
            return result;
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
