// File: Calendar.Tests/UsersControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Calendar.API;
using Calendar.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Calendar.Tests;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostUser_WhenCalled_ReturnsCreatedUser()
    {
        // Arrange
        var newUser = new User { Name = "Integration Test User", Email = "test@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", newUser);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(createdUser);
        Assert.Equal("Integration Test User", createdUser.Name);
        Assert.True(createdUser.Id > 0);
    }
}