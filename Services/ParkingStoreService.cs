using System.Collections.Concurrent;
using System.Text.Json;

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
        _parkingStore.Clear();
        try
        {
            foreach (var parking in parkings)
            {
                if (string.IsNullOrEmpty(parking.Nom))
                {
                    continue;
                }

                _parkingStore.TryAdd(parking.Nom, parking);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des parkings.");
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

    public ParkingData? GetParkingByName(string name)
    {
        try
        {
            ParkingData? parking = _parkingStore.Values
                .FirstOrDefault(e => e.Nom == name);

            if (parking != null)
            {
                return parking;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event.");
            return null;
        }
    }

    public void DeleteParking(ParkingData parkingToDelete)
    {
        var json = File.ReadAllText("parkings.json");
        var parkings = JsonSerializer.Deserialize<List<ParkingData>>(json) ?? new List<ParkingData>();

        var parkingToRemove = parkings.FirstOrDefault(p => p.Nom == parkingToDelete.Nom);

        if (parkingToRemove != null)
        {
            parkings.Remove(parkingToRemove);

            _parkingStore.TryRemove(parkingToDelete.Nom, out var removedParking);

            var updatedJson = JsonSerializer.Serialize(parkings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("parkings.json", updatedJson);
        }
    }
}
