// File: Calendar.API/Controllers/SchedulingController.cs
using Calendar.Core.Services;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

/// <summary>
/// Handles intelligent scheduling operations.
/// </summary>
[ApiController]
[Route("api/v1/scheduling")]
public class SchedulingController(ApplicationDbContext context, SchedulingService schedulingService) : ControllerBase
{
    /// <summary>
    /// Finds available time slots for a group of users.
    /// </summary>
    /// <param name="userIds">A comma-separated list of user IDs to include in the search.</param>
    /// <param name="searchStart">The start of the time window to search within.</param>
    /// <param name="searchEnd">The end of the time window to search within.</param>
    /// <param name="durationInMinutes">The required duration of the meeting in minutes.</param>
    [HttpGet("find-available-slots")]
    public async Task<IActionResult> FindAvailableSlots(
        [FromQuery] List<int> userIds, 
        [FromQuery] DateTime searchStart, 
        [FromQuery] DateTime searchEnd, 
        [FromQuery] int durationInMinutes)
    {
        if (userIds == null || !userIds.Any())
        {
            return BadRequest("At least one user ID must be provided.");
        }

        var existingEvents = await context.Events
            .Where(e => e.Participants.Any(p => userIds.Contains(p.Id)))
            .Where(e => e.EndTime > searchStart && e.StartTime < searchEnd)
            .ToListAsync();

        var availableSlots = schedulingService.FindAvailableSlots(
            existingEvents, 
            searchStart, 
            searchEnd, 
            durationInMinutes);

        return Ok(availableSlots);
    }
}