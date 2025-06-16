// File: Calendar.API/Controllers/EventsController.cs
using Calendar.Core;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController(ApplicationDbContext context) : ControllerBase
{
    // POST /api/v1/events
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
    {
        // When creating an event, you might get a list of participant IDs.
        // We need to find those users in the DB and add them to the event.
        if (newEvent.Participants.Any())
        {
            var participantIds = newEvent.Participants.Select(p => p.Id).ToList();
            var participants = await context.Users.Where(u => participantIds.Contains(u.Id)).ToListAsync();
            newEvent.Participants = participants;
        }
        
        context.Events.Add(newEvent);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
    }

    // GET /api/v1/events
    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = context.Events.Include(e => e.Participants).AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(e => e.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.EndTime <= endDate.Value);
        }

        var events = await query.ToListAsync();
        return Ok(events);
    }

    // GET /api/v1/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var calendarEvent = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (calendarEvent == null)
        {
            return NotFound();
        }
        
        return Ok(calendarEvent);
    }

    // PUT /api/v1/events/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
    {
        var existingEvent = await context.Events.FindAsync(id);
        if (existingEvent == null)
        {
            return NotFound();
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartTime = updatedEvent.StartTime;
        existingEvent.EndTime = updatedEvent.EndTime;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/v1/events/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var calendarEvent = await context.Events.FindAsync(id);
        if (calendarEvent == null)
        {
            return NotFound();
        }

        context.Events.Remove(calendarEvent);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
    
    // --- Participant Management ---

    // POST /api/v1/events/{eventId}/participants
    [HttpPost("{eventId}/participants")]
    public async Task<IActionResult> AddParticipant(int eventId, [FromBody] User participant)
    {
        var calendarEvent = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        var user = await context.Users.FindAsync(participant.Id);

        if (calendarEvent == null || user == null)
        {
            return NotFound("Event or User not found.");
        }
        
        calendarEvent.Participants.Add(user);
        await context.SaveChangesAsync();
        
        return Ok();
    }

    // DELETE /api/v1/events/{eventId}/participants/{userId}
    [HttpDelete("{eventId}/participants/{userId}")]
    public async Task<IActionResult> RemoveParticipant(int eventId, int userId)
    {
        var calendarEvent = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (calendarEvent == null)
        {
            return NotFound("Event not found.");
        }
        
        var user = calendarEvent.Participants.FirstOrDefault(p => p.Id == userId);
        if (user == null)
        {
            return NotFound("Participant not found in this event.");
        }

        calendarEvent.Participants.Remove(user);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}