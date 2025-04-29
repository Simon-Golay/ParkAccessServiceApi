using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

public class GraphService
{
    private readonly GraphServiceClient _graphClient;

    private readonly ILogger<GraphService> _logger;

    private string _clientId;
    private string _clientSecret;
    private string _tenantId;

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        _clientId = configuration["AzureAd:ClientId"];
        _clientSecret = configuration["AzureAd:ClientSecret"];
        _tenantId = configuration["AzureAd:TenantId"];

        var clientSecretCredential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });

        _logger = logger;
    }

    public async Task<IEnumerable<EventData>> GetCalendarEventsAsync(List<ParkingData> parkings)
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
                        Start = evt.Start.DateTime != null ? DateTimeOffset.Parse(evt.Start.DateTime) : (DateTimeOffset?)null,
                        End = evt.End.DateTime != null ? DateTimeOffset.Parse(evt.End.DateTime) : (DateTimeOffset?)null,
                    });
                }
            }
        }

        return eventDataList;
    }

    public async Task NewCalendarEventAsync(EventData newEvent)
    {
        var accessToken = await GetAccessToken(_tenantId, _clientId, _clientSecret);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var graphPayload = new
        {
            subject = newEvent.Name,
            start = new { dateTime = newEvent.Start.Value.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "Europe/Paris" },
            end = new { dateTime = newEvent.End.Value.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "Europe/Paris" },
            location = new { displayName = newEvent.ParkingMail }
        };

        var json = JsonConvert.SerializeObject(graphPayload);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"https://graph.microsoft.com/v1.0/users/{newEvent.ParkingMail}/events";
        var response = await httpClient.PostAsync(url, content);
    }

    static async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret)
    {
        var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();

        string[] scopes = { "https://graph.microsoft.com/.default" };
        var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
        return authResult.AccessToken;
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
            await _graphClient.Users[eventToDelete.ParkingMail].Calendar.Events[eventToDelete.Id].DeleteAsync();
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
