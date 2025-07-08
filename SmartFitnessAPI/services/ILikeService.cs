// Services/ILikeService.cs
using System.Threading.Tasks;
using SmartFitnessApi.Data.Dtos;

namespace SmartFitnessApi.Services
{
    public interface ILikeService
    {
        /// <summary>
        /// Records a like from one user to another. If a reciprocal like already exists,
        /// returns a mutual Match. Otherwise returns the pending Like.
        /// </summary>
        Task<LikeResultDto> CreateLikeAsync(int userId, int targetUserId);
        /// <summary>
        /// Retrieves all incoming likes for this user (unresponded requests).
        /// </summary>
        Task<IEnumerable<IncomingLikeDto>> GetIncomingLikesAsync(int userId);
    }
}
