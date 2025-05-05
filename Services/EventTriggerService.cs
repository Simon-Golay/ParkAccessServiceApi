using Microsoft.Kiota.Abstractions;
using ParkAccessServiceApi.Class;
using ParkAccessServiceApi.Settings;
using System.Text.Json;

public class EventTriggerService : BackgroundService
{
    private readonly ApiSettings _apiSettings;
    private readonly EventStoreService _eventStoreService;
    private readonly ILogger<EventTriggerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Dictionary<string, string> _lastStatusPerEvent = new();
    private string? status;

    public EventTriggerService(ApiSettings apiSettings, EventStoreService eventStoreService, IHttpClientFactory httpClientFactory, ILogger<EventTriggerService> logger)
    {
        _apiSettings = apiSettings;
        _eventStoreService = eventStoreService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var events = _eventStoreService.GetAllEvents();
                var now = DateTimeOffset.Now;

                foreach (var evt in events)
                {
                    if (evt.Start.HasValue && evt.End.HasValue)
                    {
                        status = "";
                        if (Math.Abs((evt.Start.Value - now).TotalSeconds) < _apiSettings.TolerenceToDetectEventInSeconds)
                        {
                            status = "on";
                        }
                        if (Math.Abs((evt.End.Value - now).TotalSeconds) < _apiSettings.TolerenceToDetectEventInSeconds)
                        {
                            status = "off";
                        }
                        if(status != "")
                        {
                            await TriggerHttpRequestAsync(evt, stoppingToken, status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking events");
            }

            await Task.Delay(_apiSettings.IntervalToControllIncomingEventInSeconds * 1000, stoppingToken);
        }
    }

    private async Task TriggerHttpRequestAsync(EventData evt, CancellationToken cancellationToken, string status)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            string url = $"http://{evt.ParkingIp}/relay/0?turn=" + status.ToString();
            HttpResponseMessage response = await client.GetAsync(url);

            if (!_lastStatusPerEvent.TryGetValue(evt.Id, out var lastStatus) || lastStatus != status)
            {
                var json = File.ReadAllText("history.json");
                var history = JsonSerializer.Deserialize<List<History>>(json) ?? new List<History>();
                var newHistory = new History(DateTime.Now, $"L'évènement \"{evt.Name}\" a déclenché le parking : {evt.ParkingMail} => {status}");
                history.Add(newHistory);

                var newJson = JsonSerializer.Serialize(history, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText("history.json", newJson);

                _lastStatusPerEvent[evt.Id] = status;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while triggering HTTP request for event {EventId}", evt.Id);
        }
    }
}
