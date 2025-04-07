using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class CalendarService : BackgroundService
{
    private readonly GraphService _graphService;
    private readonly EventStoreService _eventStoreService;
    private readonly ILogger<CalendarService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly string _url = "http://157.26.121.168:7159/api/calendar/parkings";
    private readonly HttpClient _httpClient;

    public CalendarService(GraphService graphService, EventStoreService eventStoreService, ILogger<CalendarService> logger)
    {
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
                HttpResponseMessage response = await _httpClient.GetAsync(_url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json, options);

                if (parkings != null)
                {
                    var events = await _graphService.GetResourceCalendarEventsAsync(parkings);
                    _eventStoreService.UpdateEvents(events);
                    _logger.LogInformation("Calendar events updated at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling calendar events");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
}
