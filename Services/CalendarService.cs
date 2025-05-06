using ParkAccessServiceApi.Settings;
using System.Text.Json;

public class CalendarService : BackgroundService
{
    private readonly ApiSettings _apiSettings;
    private readonly GraphService _graphService;
    private readonly EventStoreService _eventStoreService;
    private readonly ILogger<CalendarService> _logger;
    private readonly HttpClient _httpClient;

    public CalendarService(ApiSettings apiSettings, GraphService graphService, EventStoreService eventStoreService, ILogger<CalendarService> logger)
    {
        _apiSettings = apiSettings;
        _graphService = graphService;
        _eventStoreService = eventStoreService;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var json = File.ReadAllText("parkings.json");
                var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json) ?? new List<ParkingData>();

                if (parkings != null)
                {
                    var events = await _graphService.GetCalendarEventsAsync(parkings);
                    _eventStoreService.UpdateEvents(events);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ExecuteAsync");
            }

            await Task.Delay(_apiSettings.IntervalToGetEventsFromCalendarInSeconds, stoppingToken);
        }
    }
}
