// File: Calendar.API/Program.cs
using Calendar.Core.Services;
using Calendar.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // Often needed for SwaggerDoc options
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- Service Configuration ---

// 1. Add CORS services to allow external applications (like a Blazor UI) to call this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        policy => policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

// 2. Add services for controllers and API exploration.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Configure Swagger/OpenAPI for API documentation.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Friendly Calendar API", Version = "v1" });

    // Use reflection to find the XML file that contains the controller documentation.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// 4. Configure the database connection using Entity Framework Core with SQLite.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString, b => b.MigrationsAssembly("Calendar.API")));

// 5. Register custom application services for dependency injection.
builder.Services.AddScoped<SchedulingService>();


// --- Application Pipeline Configuration ---

var app = builder.Build();

// Configure the HTTP request pipeline.
// In a development environment, enable Swagger and the Swagger UI.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Calendar API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enable the CORS policy. This must be called before UseAuthorization.
app.UseCors("AllowAnyOrigin");

// Enable serving static files from the wwwroot folder (for ai-plugin.json).
app.UseStaticFiles();

// Enable authorization middleware (though we haven't configured specific policies yet).
app.UseAuthorization();

// Map controller endpoints.
app.MapControllers();

// Run the application.
app.Run();


/// <summary>
/// The main entry point for the application.
/// This partial class declaration makes the auto-generated Program class public,
/// which is a requirement for the WebApplicationFactory used in integration tests.
/// </summary>
public partial class Program { }