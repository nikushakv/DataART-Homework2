// File: Calendar.Tests/UsersControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Calendar.API;
using Calendar.Core;
using Calendar.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calendar.Tests;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;
    private readonly IServiceScope _scope;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        // Create a single, shared in-memory SQLite connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove the original DbContext options
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Add a new DbContext using the shared in-memory connection
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });
        });

        // Create a scope to get services and ensure the database is created
        _scope = _factory.Services.CreateScope();
        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task PostUser_WhenCalled_ReturnsCreatedUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newUser = new User { Name = "Integration Test User", Email = "test@example.com" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", newUser);
        
        // Assert
        response.EnsureSuccessStatusCode(); // Throws an exception if not 2xx
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(createdUser);
        Assert.Equal("Integration Test User", createdUser.Name);
        Assert.True(createdUser.Id > 0);
    }

    // This Dispose method is crucial to clean up the database connection after tests are done
    public void Dispose()
    {
        _scope.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}