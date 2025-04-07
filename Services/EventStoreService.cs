using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

public class EventStoreService
{
    private readonly ConcurrentDictionary<string, EventData> _eventStore = new();
    private readonly ILogger<EventStoreService> _logger;

    public EventStoreService(ILogger<EventStoreService> logger)
    {
        _logger = logger;
    }

    public void UpdateEvents(IEnumerable<EventData> events)
    {
        foreach (var evt in events)
        {
            if (evt.Start.HasValue)
            {
                evt.Start = evt.Start.Value.AddHours(2);
            }
            if (evt.End.HasValue)
            {
                evt.End = evt.End.Value.AddHours(2);
            }
            _eventStore.AddOrUpdate(evt.Id, evt, (key, existing) => evt);
        }
    }

    public IEnumerable<EventData> GetAllEvents()
    {
        try
        {
            return _eventStore.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events.");
            return new List<EventData>();
        }
    }

    public EventData GetNextEvent()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;

            var currentEvent = _eventStore.Values
                .FirstOrDefault(e => e.Start.HasValue && e.End.HasValue &&
                                    e.Start.Value <= now && e.End.Value >= now);

            if (currentEvent != null)
            {
                return currentEvent;
            }

            var nextEvent = _eventStore.Values
                .Where(e => e.Start.HasValue && e.Start.Value > now)
                .OrderBy(e => e.Start.Value)
                .FirstOrDefault();

            if (nextEvent != null)
            {
                return nextEvent;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events.");
            return null;
        }
    }
}
