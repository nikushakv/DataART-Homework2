// File: Calendar.API/Controllers/UsersController.cs
using Calendar.Core;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    // POST /api/v1/users - Create a new user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest();
        }

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    // GET /api/v1/users - Get all users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await context.Users.ToListAsync();
        return Ok(users);
    }

    // GET /api/v1/users/{id} - Get a single user by their ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // PUT /api/v1/users/{id} - Update an existing user
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
    {
        if (id != updatedUser.Id)
        {
            return BadRequest();
        }

        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        
        await context.SaveChangesAsync();
        return NoContent(); // Success, no content to return
    }

    // DELETE /api/v1/users/{id} - Delete a user
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        return NoContent(); // Success, no content to return
    }
}