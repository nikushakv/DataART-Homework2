// File: Calendar.Infrastructure/ApplicationDbContext.cs
using Calendar.Core;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // This sets up the many-to-many relationship table automatically.
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Participants)
            .WithMany(u => u.Events);
    }
}