using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System.Net.Http;

public class OnOffService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OnOffService> _logger;
    private readonly Random _random = new Random();
    private string? status;

    public OnOffService(IHttpClientFactory httpClientFactory, ILogger<OnOffService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (status == "on")
                {
                    status = "off";
                }
                else if (status == "off")
                {
                    status = "on";
                }
                else
                {
                    status = "on";
                }
                var client = _httpClientFactory.CreateClient();

                string url = $"http://157.26.121.184/relay/0?turn={status}";
                await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while OnOff");
            }

            int delayMs = _random.Next(10, 1500);

            await Task.Delay(delayMs, stoppingToken);
        }
    }
}
