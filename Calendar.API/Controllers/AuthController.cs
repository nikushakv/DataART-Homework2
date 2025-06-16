// File: Calendar.API/Controllers/AuthController.cs
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Controllers;

/// <summary>
/// Controller for handling user authentication
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController(ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Represents a login request containing user credentials
    /// </summary>
    /// <param name="Email">The user's email address</param>
    public record LoginRequest(string Email);

    /// <summary>
    /// Logs a user in. This is a simplified login that just checks if the user exists.
    /// </summary>
    /// <param name="request">The login request containing the user's email.</param>
    /// <returns>A confirmation message if the user exists.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">User not found or invalid credentials.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // In a real application, you would generate and return a JWT (JSON Web Token) here.
        // For this project, a simple success message is sufficient.
        return Ok(new { Message = $"Login successful for user {user.Name}", UserId = user.Id });
    }
}