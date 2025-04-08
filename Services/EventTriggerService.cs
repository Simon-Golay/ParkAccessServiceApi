using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class EventTriggerService : BackgroundService
{
    private readonly EventStoreService _eventStoreService;
    private readonly ILogger<EventTriggerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _tolerance = TimeSpan.FromSeconds(20);
    private string? status;

    public EventTriggerService(EventStoreService eventStoreService, IHttpClientFactory httpClientFactory, ILogger<EventTriggerService> logger)
    {
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
                        if (Math.Abs((evt.Start.Value - now).TotalSeconds) < _tolerance.TotalSeconds)
                        {
                            status = "on";
                        }
                        if (Math.Abs((evt.End.Value - now).TotalSeconds) < _tolerance.TotalSeconds)
                        {
                            status = "off";
                        }
                        if(status != "")
                        {
                            _logger.LogInformation("Triggering HTTP request executed");
                            await TriggerHttpRequestAsync(evt, stoppingToken, status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking events");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("EventTriggerService stopping.");
    }

    private async Task TriggerHttpRequestAsync(EventData evt, CancellationToken cancellationToken, string status)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            string url = $"http://157.26.121.184/relay/0?turn=" + status.ToString();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("HTTP request triggered successfully");
            }
            else
            {
                _logger.LogWarning("HTTP request failed for event {EventId} with status {StatusCode}", evt.Id, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while triggering HTTP request for event {EventId}", evt.Id);
        }
    }
}
