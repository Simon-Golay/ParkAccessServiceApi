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
                string _url = $"{_apiSettings.BaseUrl}/parkings";
                var request = new HttpRequestMessage(HttpMethod.Get, _url);
                request.Headers.Add("ApiKey", _apiSettings.ApiKey);

                HttpResponseMessage response = await _httpClient.SendAsync(request, stoppingToken);

                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json, options);

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
