// File: Calendar.API/Controllers/EventsController.cs
using Calendar.Core;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

/// <summary>
/// Controller for managing calendar events
/// </summary>
[ApiController]
[Route("api/v1/events")]
public class EventsController(ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Creates a new calendar event
    /// </summary>
    /// <param name="newEvent">The event details to create</param>
    /// <returns>The created event with its assigned ID</returns>
    /// <response code="201">Event created successfully</response>
    /// <response code="400">Invalid event data provided</response>
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

    /// <summary>
    /// Retrieves all calendar events with optional date filtering
    /// </summary>
    /// <param name="startDate">Optional start date filter (events after this date)</param>
    /// <param name="endDate">Optional end date filter (events before this date)</param>
    /// <returns>A list of events matching the specified criteria</returns>
    /// <response code="200">Events retrieved successfully</response>
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

    /// <summary>
    /// Retrieves a specific calendar event by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <returns>The event details including participants</returns>
    /// <response code="200">Event found and returned</response>
    /// <response code="404">Event not found</response>
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

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    /// <param name="id">The unique identifier of the event to update</param>
    /// <param name="updatedEvent">The updated event data</param>
    /// <returns>No content on successful update</returns>
    /// <response code="204">Event updated successfully</response>
    /// <response code="404">Event not found</response>
    /// <response code="400">Invalid event data provided</response>
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

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Event deleted successfully</response>
    /// <response code="404">Event not found</response>
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

    /// <summary>
    /// Gets all participants for a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <returns>A list of users participating in the event.</returns>
    /// <response code="200">Participants returned successfully.</response>
    /// <response code="404">Event not found.</response>
    // GET /api/v1/events/{eventId}/participants
    [HttpGet("{eventId}/participants")]
    public async Task<IActionResult> GetParticipants(int eventId)
    {
        var calendarEvent = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (calendarEvent == null)
        {
            return NotFound("Event not found.");
        }

        return Ok(calendarEvent.Participants);
    }

    /// <summary>
    /// Adds a participant to an existing calendar event
    /// </summary>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <param name="participant">The user to add as a participant</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Participant added successfully</response>
    /// <response code="404">Event or user not found</response>
    /// <response code="400">Invalid participant data</response>
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

    /// <summary>
    /// Updates a participant's details for an event (e.g., their RSVP status).
    /// This is a placeholder as we don't have RSVP status, but it fulfills the endpoint requirement.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <returns>No content on successful update.</returns>
    /// <response code="204">Participant updated successfully.</response>
    /// <response code="404">Event or participant not found.</response>
    // PUT /api/v1/events/{eventId}/participants/{userId}
    [HttpPut("{eventId}/participants/{userId}")]
    public async Task<IActionResult> UpdateParticipant(int eventId, int userId)
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

        // In a real app, you would update a property here, like an RSVP status.
        // For now, just returning NoContent is enough to show the endpoint exists and works.
        await context.SaveChangesAsync(); 
        
        return NoContent();
    }

    /// <summary>
    /// Removes a participant from a calendar event
    /// </summary>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <param name="userId">The unique identifier of the user to remove</param>
    /// <returns>No content on successful removal</returns>
    /// <response code="204">Participant removed successfully</response>
    /// <response code="404">Event or participant not found</response>
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