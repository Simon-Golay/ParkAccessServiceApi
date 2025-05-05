using Microsoft.Graph.Models;
using ParkAccessServiceApi.Class;
using System.Collections.Concurrent;
using System.Text.Json;

public class EventStoreService
{
    private readonly ConcurrentDictionary<string, EventData> _eventStore = new();
    private readonly ILogger<EventStoreService> _logger;

    private readonly GraphService _graphService;

    public EventStoreService(ILogger<EventStoreService> logger, GraphService graphService)
    {
        _logger = logger;
        _graphService = graphService;
    }

    public void UpdateEvents(IEnumerable<EventData> events)
    {
        var newEvents = new List<EventData>();
        var removedEvents = new List<EventData>();

        var oldEventIds = _eventStore.Keys.ToHashSet();
        var incomingEventIds = new HashSet<string>();

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

            incomingEventIds.Add(evt.Id);

            if (!_eventStore.ContainsKey(evt.Id))
            {
                newEvents.Add(evt);
            }
        }

        foreach (var oldId in oldEventIds)
        {
            if (!incomingEventIds.Contains(oldId))
            {
                if (_eventStore.TryGetValue(oldId, out var removedEvt))
                {
                    removedEvents.Add(removedEvt);
                }
            }
        }

        var jsonPath = "history.json";
        var jsonContent = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "[]";
        var history = JsonSerializer.Deserialize<List<History>>(jsonContent) ?? new List<History>();

        foreach (var ne in newEvents)
        {
            history.Add(new History(DateTime.Now, $"L'évènement \"{ne.Name}\" du parking \"{ne.ParkingMail}\" a été ajouté avec succès"));
        }
        foreach (var re in removedEvents)
        {
            history.Add(new History(DateTime.Now, $"L'évènement \"{re.Name}\" du parking \"{re.ParkingMail}\" a été supprimé avec succès"));
        }

        var updatedJson = JsonSerializer.Serialize(history, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(jsonPath, updatedJson);

        _eventStore.Clear();
        foreach (var evt in events)
        {
            _eventStore[evt.Id] = evt;
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

    public void AddNewEvent(EventData newEvent)
    {
        try
        {
            _graphService.NewCalendarEventAsync(newEvent).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new event.");
        }
    }

    public EventData? GetEventByName(string name)
    {
        try
        {
            EventData? nextEvent = _eventStore.Values
                .FirstOrDefault(e => e.Name == name);

            if (nextEvent != null)
            {
                return nextEvent;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event.");
            return null;
        }
    }

    public void DeleteEvent(EventData eventToDelete)
    {
        try
        {
            _graphService.DeleteEventFromCalendarAsync(eventToDelete).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event.");
        }
    }
}
