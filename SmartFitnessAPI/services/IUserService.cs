// Services/IUsersService.cs
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IUsersService
    {
        /// <summary>
        /// Returns the filtered & sorted “queue” of potential partners for the given user.
        /// </summary>
        /// <param name="currentUserId">internal numeric user ID from Claims</param>
        /// <param name="sortBy">Recent | Compatibility | Interaction | All</param>
        Task<IEnumerable<UserQueueDto>> GetUserQueueAsync(
            int currentUserId,
            string sortBy = "Recent"
        );
    }
}
