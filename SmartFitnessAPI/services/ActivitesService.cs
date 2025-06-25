// Services/ActivityService.cs
using SmartFitnessApi.Data;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

public class ActivityService : IActivitesService
{
    private readonly SmartFitnessDbContext _context;

    public ActivityService(SmartFitnessDbContext context)
    {
        _context = context;
    }

    // public async Task<List<ActivityDto>> GetActivitiesAsync(int userId, string type = null, bool IsActive = true)
    // {
    //     var query = _context.Activities;

    //     if (!string.IsNullOrWhiteSpace(type))
    //         query = query.Where(a => a. == type);

    //     if (!string.IsNullOrWhiteSpace(status))
    //         query = query.Where(a => a.IsActive == IsActive);

    //     return await query
    //         .OrderByDescending(a => a.Date)
    //         .Select(a => new ActivityDto
    //         {
    //             Id = a.Id,
    //             Type = a.Type,
    //             Status = a.Status,
    //             Duration = a.Duration,
    //             CaloriesBurned = a.CaloriesBurned,
    //             Date = a.Date
    //         })
    //         .ToListAsync();
    // }
}
