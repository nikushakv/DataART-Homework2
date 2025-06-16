// File: Calendar.API/Controllers/SchedulingController.cs
using Calendar.Core;
using Calendar.Core.Services;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

[ApiController]
[Route("api/v1/scheduling")]
public class SchedulingController(ApplicationDbContext context, SchedulingService schedulingService) : ControllerBase
{
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