// File: Calendar.Core/SchedulingService.cs
using Calendar.Core;

namespace Calendar.Core.Services;

public class SchedulingService
{
    public List<TimeSlot> FindAvailableSlots(
        List<Event> existingEvents, 
        DateTime searchStart, 
        DateTime searchEnd, 
        int durationInMinutes)
    {
        // 1. Create a single list of all "busy" time blocks.
        var busySlots = existingEvents
            .Select(e => new TimeSlot(e.StartTime, e.EndTime))
            .OrderBy(s => s.StartTime)
            .ToList();

        // 2. Merge overlapping busy slots.
        var mergedBusySlots = new List<TimeSlot>();
        if (busySlots.Any())
        {
            var currentMerge = busySlots.First();
            foreach (var slot in busySlots.Skip(1))
            {
                if (slot.StartTime < currentMerge.EndTime) // They overlap
                {
                    // Extend the current merge if the new slot ends later.
                    if (slot.EndTime > currentMerge.EndTime)
                    {
                        currentMerge = new TimeSlot(currentMerge.StartTime, slot.EndTime);
                    }
                }
                else // No overlap, start a new merge.
                {
                    mergedBusySlots.Add(currentMerge);
                    currentMerge = slot;
                }
            }
            mergedBusySlots.Add(currentMerge);
        }

        // 3. Find the gaps between the busy slots.
        var availableSlots = new List<TimeSlot>();
        var currentTime = searchStart;

        foreach (var busySlot in mergedBusySlots)
        {
            // Check the gap between the current time and the start of the next busy slot.
            if (currentTime < busySlot.StartTime)
            {
                var freeSlot = new TimeSlot(currentTime, busySlot.StartTime);
                if ((freeSlot.EndTime - freeSlot.StartTime).TotalMinutes >= durationInMinutes)
                {
                    availableSlots.Add(freeSlot);
                }
            }
            // Move our search time to the end of the busy slot.
            currentTime = busySlot.EndTime;
        }

        // 4. Check the final gap after the last busy slot until the end of the search window.
        if (currentTime < searchEnd)
        {
            var finalSlot = new TimeSlot(currentTime, searchEnd);
            if ((finalSlot.EndTime - finalSlot.StartTime).TotalMinutes >= durationInMinutes)
            {
                availableSlots.Add(finalSlot);
            }
        }

        return availableSlots;
    }
}