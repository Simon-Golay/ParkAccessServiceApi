using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class ParkingStoreService
{
    private readonly ConcurrentDictionary<string, ParkingData> _parkingStore = new();
    private readonly ILogger<ParkingStoreService> _logger;
    public ParkingStoreService(ILogger<ParkingStoreService> logger)
    {
        _logger = logger;
    }

    public void UpdateParkings(IEnumerable<ParkingData> parkings)
    {
        foreach (var parking in parkings)
        {
            _parkingStore.Clear();
            _parkingStore.TryAdd(parking.Nom, parking);
        }
    }

    public IEnumerable<ParkingData> GetAllParkings()
    {
        try
        {
            var json = File.ReadAllText("parkings.json");
            var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json) ?? new List<ParkingData>();

            if(parkings != null)
                UpdateParkings(parkings);

            return parkings ?? new List<ParkingData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parkings from parkings.json");
            return new List<ParkingData>();
        }
    }

    public void AddNewParking(ParkingData newParking)
    {
        try
        {
            var json = File.ReadAllText("parkings.json");
            var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json) ?? new List<ParkingData>();

            parkings.Add(newParking);
            _parkingStore.TryAdd(newParking.Nom, newParking);

            var updatedJson = JsonSerializer.Serialize(parkings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("parkings.json", updatedJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new parking to parkings.json");
        }
    }
}
