
// Services/IProfileService.cs
using System.Threading.Tasks;
using SmartFitnessApi.Data.Dtos;

namespace SmartFitnessApi.Services
{
    public interface IProfileService
    {
        /// <summary>
        /// Retrieves the profile information for a given user id.
        /// </summary>
        Task<ProfileMatchDto?> GetProfileAsync(int userId);
    }
}
