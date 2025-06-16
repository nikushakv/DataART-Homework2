// File: Calendar.API/Controllers/UsersController.cs
using Calendar.Core;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

/// <summary>
/// Manages user-related operations.
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Gets a list of all users.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await context.Users.ToListAsync());
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await context.Users.FindAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        await context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a user by their ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return NoContent();
    }
}