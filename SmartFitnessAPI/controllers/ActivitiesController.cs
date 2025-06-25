using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/match-requests")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivitesService _activitesService;

        public ActivitiesController(IActivitesService activitesService)
        {
            _activitesService = activitesService;
        }
        /// <summary>
        /// GET api/activities?type={type}&status={status}
        /// Returns all activities for the current user, optionally filtered by type and/or status.
        /// </summary>
        // [HttpGet]
        // public async Task<ActionResult<List<ActivityDto>>> GetActivities(
        //     [FromQuery] string type = null,
        //     [FromQuery] string status = null)
        // {
        //     var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (!int.TryParse(raw, out var userId) || userId == 0)
        //         return Unauthorized();

        //     var activities = await _activitesService.GetActivitiesAsync(userId, type, status);
        //     return Ok(activities);
        // }
    }
}
