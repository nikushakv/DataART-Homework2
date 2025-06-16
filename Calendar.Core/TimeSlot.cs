// File: Calendar.Core/TimeSlot.cs
namespace Calendar.Core;

// A simple record to represent an available time slot.
public record TimeSlot(DateTime StartTime, DateTime EndTime);