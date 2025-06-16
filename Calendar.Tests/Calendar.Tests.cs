// File: Calendar.Tests/SchedulingServiceTests.cs
using Calendar.Core;
using Calendar.Core.Services;

namespace Calendar.Tests;

public class SchedulingServiceTests
{
    private readonly SchedulingService _service = new();

    [Fact]
    public void FindAvailableSlots_NoExistingEvents_ReturnsFullTimeRange()
    {
        // Arrange
        var existingEvents = new List<Event>();
        var searchStart = new DateTime(2024, 1, 1, 9, 0, 0);
        var searchEnd = new DateTime(2024, 1, 1, 17, 0, 0);
        var duration = 60;

        // Act
        var slots = _service.FindAvailableSlots(existingEvents, searchStart, searchEnd, duration);

        // Assert
        Assert.Single(slots);
        Assert.Equal(searchStart, slots[0].StartTime);
        Assert.Equal(searchEnd, slots[0].EndTime);
    }

    [Fact]
    public void FindAvailableSlots_OneEventInMiddle_ReturnsTwoSlots()
    {
        // Arrange
        var existingEvents = new List<Event>
        {
            new() { StartTime = new DateTime(2024, 1, 1, 12, 0, 0), EndTime = new DateTime(2024, 1, 1, 13, 0, 0) }
        };
        var searchStart = new DateTime(2024, 1, 1, 9, 0, 0);
        var searchEnd = new DateTime(2024, 1, 1, 17, 0, 0);
        var duration = 60;

        // Act
        var slots = _service.FindAvailableSlots(existingEvents, searchStart, searchEnd, duration);

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.Equal(new DateTime(2024, 1, 1, 9, 0, 0), slots[0].StartTime);
        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), slots[0].EndTime);
        Assert.Equal(new DateTime(2024, 1, 1, 13, 0, 0), slots[1].StartTime);
        Assert.Equal(new DateTime(2024, 1, 1, 17, 0, 0), slots[1].EndTime);
    }
    
    [Fact]
    public void FindAvailableSlots_WithOverlappingEvents_MergesAndFindsCorrectSlots()
    {
        // Arrange
        var existingEvents = new List<Event>
        {
            // These two events overlap and should be treated as one block from 10:00 to 11:30
            new() { StartTime = new DateTime(2024, 1, 1, 10, 0, 0), EndTime = new DateTime(2024, 1, 1, 11, 0, 0) },
            new() { StartTime = new DateTime(2024, 1, 1, 10, 30, 0), EndTime = new DateTime(2024, 1, 1, 11, 30, 0) }
        };
        var searchStart = new DateTime(2024, 1, 1, 9, 0, 0);
        var searchEnd = new DateTime(2024, 1, 1, 13, 0, 0);
        var duration = 30;

        // Act
        var slots = _service.FindAvailableSlots(existingEvents, searchStart, searchEnd, duration);

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.Equal(new DateTime(2024, 1, 1, 9, 0, 0), slots[0].StartTime);
        Assert.Equal(new DateTime(2024, 1, 1, 10, 0, 0), slots[0].EndTime);
        Assert.Equal(new DateTime(2024, 1, 1, 11, 30, 0), slots[1].StartTime);
        Assert.Equal(new DateTime(2024, 1, 1, 13, 0, 0), slots[1].EndTime);
    }
}