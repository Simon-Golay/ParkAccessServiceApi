using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GraphService
{
    private readonly GraphServiceClient _graphClient;

    private readonly ILogger<GraphService> _logger;

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        var tenantId = configuration["AzureAd:TenantId"];

        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });

        _logger = logger;
    }

    // Méthode pour récupérer les événements d'un calendrier de ressource par email
    public async Task<IEnumerable<EventData>> GetResourceCalendarEventsAsync(string resourceEmail)
    {
        var result = await _graphClient.Users[resourceEmail].Calendar.Events.GetAsync();

        var eventDataList = new List<EventData>();

        if (result?.Value != null)
        {
            foreach (var evt in result.Value)
            {
                //_logger.LogInformation("Event found: {eventId}, Start: {start}, End: {end}", evt.Id, evt.Start?.DateTime, evt.End?.DateTime);
                eventDataList.Add(new EventData
                {
                    Id = evt.Id,
                    Start = evt.Start?.DateTime != null ? DateTimeOffset.Parse(evt.Start.DateTime) : (DateTimeOffset?)null,
                    End = evt.End?.DateTime != null ? DateTimeOffset.Parse(evt.End.DateTime) : (DateTimeOffset?)null,
                });
            }
        }
        else
        {
            _logger.LogWarning("No events found for resource: {resourceEmail}", resourceEmail);
        }

        return eventDataList;
    }

}
