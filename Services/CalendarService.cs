using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CalendarService : BackgroundService
{
    private readonly GraphService _graphService;
    private readonly EventStoreService _eventStoreService;
    private readonly ILogger<CalendarService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly string _resourceEmail = "parking.test@iceff.ch";

    public CalendarService(GraphService graphService, EventStoreService eventStoreService, ILogger<CalendarService> logger)
    {
        _graphService = graphService;
        _eventStoreService = eventStoreService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var events = await _graphService.GetResourceCalendarEventsAsync(_resourceEmail);
                _eventStoreService.UpdateEvents(events);
                _logger.LogInformation("Calendar events updated at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling calendar events");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
}
