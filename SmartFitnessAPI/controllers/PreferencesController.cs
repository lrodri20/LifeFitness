using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PreferencesController : ControllerBase
    {
        private readonly IPreferencesService _preferencesService;

        public PreferencesController(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;
        }

        /// <summary>
        /// Get the current user's matching preferences
        /// </summary>
        /// <returns>The user's matching preferences</returns>
        [HttpGet]
        public async Task<ActionResult<MatchingPreferenceDto>> GetMatchingPreferences()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

            try
            {
                var preferences = await _preferencesService.GetMatchingPreferencesAsync(userId);
                return Ok(preferences);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching preferences");
            }
        }

        /// <summary>
        /// Create or update the current user's matching preferences
        /// </summary>
        /// <param name="preferences">The matching preferences to save</param>
        /// <returns>The saved matching preferences</returns>
        [HttpPut]
        public async Task<ActionResult<MatchingPreferenceDto>> UpdateMatchingPreferences(
            [FromBody] UpdateMatchingPreferenceDto preferences)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _preferencesService.UpdateMatchingPreferencesAsync(userId, preferences);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating preferences");
            }
        }

        /// <summary>
        /// Delete the current user's matching preferences (reset to defaults)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete]
        public async Task<ActionResult> DeleteMatchingPreferences()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

            try
            {
                await _preferencesService.DeleteMatchingPreferencesAsync(userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting preferences");
            }
        }
    }
}