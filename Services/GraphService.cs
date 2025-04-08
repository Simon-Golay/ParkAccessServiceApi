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

    public async Task<IEnumerable<EventData>> GetResourceCalendarEventsAsync(List<ParkingData> parkings)
    {
        var eventDataList = new List<EventData>();

        for(int i = 0; i < parkings.Count(); i++)
        {
            if (string.IsNullOrWhiteSpace(parkings[i].Mail) || !IsValidEmail(parkings[i].Mail))
            {
                parkings.Remove(parkings[i]);
            }
        }

        foreach (ParkingData parking in parkings)
        {
            var result = await _graphClient.Users[parking.Mail].Calendar.Events.GetAsync();

            if (result?.Value != null)
            {
                foreach (var evt in result.Value)
                {
                    eventDataList.Add(new EventData
                    {
                        Id = evt.Id,
                        Name = evt.Subject,
                        ParkingMail = parking.Mail,
                        ParkingIp = parking.Ip,
                        Start = evt.Start?.DateTime != null ? DateTimeOffset.Parse(evt.Start.DateTime) : (DateTimeOffset?)null,
                        End = evt.End?.DateTime != null ? DateTimeOffset.Parse(evt.End.DateTime) : (DateTimeOffset?)null,
                    });
                }
            }
        }

        return eventDataList;
    }

    public async Task DeleteEventFromCalendarAsync(EventData eventToDelete)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventToDelete.ParkingMail) || !IsValidEmail(eventToDelete.ParkingMail))
            {
                _logger.LogWarning($"Email incorrect pour le parking: {eventToDelete.ParkingMail}");
                return;
            }

            // Supprimer l'événement du calendrier de l'utilisateur
            await _graphClient.Users[eventToDelete.ParkingMail].Calendar.Events[eventToDelete.Id].DeleteAsync();

            _logger.LogInformation($"Event with Name {eventToDelete.Name} successfully deleted from calendar of {eventToDelete.ParkingMail}");
        }
        catch (ServiceException ex)
        {
            _logger.LogError($"Error deleting event from Outlook calendar: {ex.Message}");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return mail.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
