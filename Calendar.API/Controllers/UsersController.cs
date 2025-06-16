// File: Calendar.API/Controllers/SchedulingController.cs
using Calendar.Core;
using Calendar.Core.Services;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

/// <summary>
/// Controller for intelligent scheduling and finding available time slots
/// </summary>
[ApiController]
[Route("api/v1/scheduling")]
public class SchedulingController(ApplicationDbContext context, SchedulingService schedulingService) : ControllerBase
{
    /// <summary>
    /// Finds available time slots for multiple users within a specified time range
    /// </summary>
    /// <param name="userIds">List of user IDs to check availability for</param>
    /// <param name="searchStart">Start date and time for the search window</param>
    /// <param name="searchEnd">End date and time for the search window</param>
    /// <param name="durationInMinutes">Required duration for the meeting in minutes</param>
    /// <returns>A list of available time slots that work for all specified users</returns>
    /// <response code="200">Available slots found and returned</response>
    /// <response code="400">Invalid parameters provided (e.g., no user IDs specified)</response>
    /// <response code="500">Error occurred while processing the request</response>
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

        // Fetch all events for the specified users within the search window
        var existingEvents = await context.Events
            .Where(e => e.Participants.Any(p => userIds.Contains(p.Id)))
            .Where(e => e.EndTime > searchStart && e.StartTime < searchEnd)
            .ToListAsync();

        // Use the service to find available slots
        var availableSlots = schedulingService.FindAvailableSlots(
            existingEvents, 
            searchStart, 
            searchEnd, 
            durationInMinutes);

        return Ok(availableSlots);
    }
}