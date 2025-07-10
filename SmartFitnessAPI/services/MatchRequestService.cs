namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Data.Dtos;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class MatchRequestService : IMatchRequestService
    {
        private readonly SmartFitnessDbContext _context;

        public MatchRequestService(SmartFitnessDbContext context)
        {
            _context = context;
        }

        public async Task<MatchRequest> CreateMatchRequestAsync(int requesterId, int requesteeId, string initialMessage = null)
        {
            // Validate requester
            var requester = await _context.Users.FindAsync(requesterId);
            if (requester == null)
                throw new InvalidOperationException("Requester user does not exist.");

            // Validate requestee
            if (requesteeId <= 0)
                throw new ArgumentException("Invalid requestee user ID.", nameof(requesteeId));
            // Check if receiver exists
            var receiver = await _context.Users.FindAsync(requesteeId);
            if (receiver == null)
                throw new InvalidOperationException("Target user does not exist.");

            // Prevent duplicate requests
            var existingRequest = await _context.MatchRequests
                .FirstOrDefaultAsync(r => r.RequesterId == requesterId && r.RequesteeId == requesteeId);

            if (existingRequest != null)
                throw new InvalidOperationException("You have already sent a request to this user.");

            var request = new MatchRequest
            {
                RequesterId = requesterId,
                RequesteeId = requesteeId,
                CreatedAt = DateTime.UtcNow,
                Status = MatchStatus.Pending,
                InitialMessage = initialMessage // Assuming no initial message is provided
            };

            _context.MatchRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }
        public async Task<List<MatchRequestDto>> GetMatchRequestsAsync(int userId, string direction, string status)
        {
            IQueryable<MatchRequest> query = _context.MatchRequests
                .Include(r => r.Requestee)
                .Include(r => r.Requester);

            direction = direction.ToLower();
            status = status?.ToLower();

            // Apply direction filtering
            query = direction switch
            {
                "incoming" => query.Where(r => r.RequesteeId == userId),
                "outgoing" => query.Where(r => r.RequesterId == userId),
                "all" => query.Where(r => r.RequesteeId == userId || r.RequesterId == userId),
                _ => throw new ArgumentException("Invalid direction. Must be 'incoming', 'outgoing', or 'all'.")
            };

            // Apply status filtering if provided
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status.ToString() == status);

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Map to DTO
            return requests.Select(r => new MatchRequestDto
            {
                Id = r.Id,
                RequesteeId = r.RequesteeId,
                RequesterId = r.RequesterId,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                RequesterName = r.Requester?.FirstName + " " + r.Requester?.LastName,
                RequesteeName = r.Requestee?.FirstName + " " + r.Requestee?.LastName
            }).ToList();
        }
        /// <summary>
        /// Returns all pending match‐requests where the given user is the requestee.
        /// </summary>
        public async Task<IEnumerable<MatchRequest>> GetPendingRequestsAsync(int requesteeId)
        {
            return await _context.MatchRequests
                .Include(r => r.Requester)
                .Include(r => r.Requestee)
                .Where(r => r.RequesteeId == requesteeId
                            && r.Status == MatchStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        public async Task AcceptMatchRequestAsync(int requestId, int requesteeId)
        {
            // 1) Load the request, ensure it belongs to this requestee
            var request = await _context.MatchRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.RequesteeId == requesteeId);

            if (request == null)
                throw new InvalidOperationException("Match request not found or you are not the requestee.");

            if (request.Status != MatchStatus.Pending)
                throw new InvalidOperationException("Match request has already been processed.");

            // 2) Update its status
            request.Status = MatchStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            // 3) (Optional) if you have a Matches table, create the mutual match record here:
            // var match = new Match {
            //     User1Id = request.RequesterId,
            //     User2Id = request.RequesteeId,
            //     CreatedAt = DateTime.UtcNow
            // };
            // _context.Matches.Add(match);

            // 4) Persist
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Marks the given pending request as “Rejected” (only the requestee may call this).
        /// </summary>
        /// <param name="requestId">The ID of the MatchRequest to reject.</param>
        /// <param name="userId">The user ID of the currently‐logged‐in requestee.</param>
        public async Task RejectMatchRequestAsync(int requestId, int userId)
        {
            // 1) Load the request, ensure it belongs to this requestee
            var request = await _context.MatchRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.RequesteeId == userId);

            if (request == null)
                throw new InvalidOperationException("Match request not found or you are not the requestee.");

            if (request.Status != MatchStatus.Pending)
                throw new InvalidOperationException("Match request has already been processed.");

            // 2) Update its status
            request.Status = MatchStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            // 3) Persist changes
            await _context.SaveChangesAsync();
        }
    }
}