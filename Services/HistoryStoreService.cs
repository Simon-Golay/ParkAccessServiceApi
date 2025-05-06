using ParkAccessServiceApi.Class;
using System.Collections.Concurrent;
using System.Text.Json;

public class HistoryStoreService
{
    private readonly ConcurrentDictionary<string, History> _historyStore = new();
    private readonly ILogger<HistoryStoreService> _logger;
    private static readonly SemaphoreSlim FileLock = new(1, 1);
    public HistoryStoreService(ILogger<HistoryStoreService> logger)
    {
        _logger = logger;
    }

    public void UpdateHistory(IEnumerable<History> history)
    {
        _historyStore.Clear();
        try
        {
            foreach (var h in history)
            {
                if (string.IsNullOrEmpty(h.Description))
                {
                    continue;
                }

                _historyStore.TryAdd(h.Description, h);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public IEnumerable<History> GetHistory()
    {
        try
        {
            var json = File.ReadAllText("history.json");
            var history = JsonSerializer.Deserialize<List<History>>(json) ?? new List<History>();

            if (history != null)
                UpdateHistory(history);

            return history ?? new List<History>();
        }
        catch (Exception ex)
        {
            return new List<History>();
        }
    }

    public void DeleteHistory()
    {
        try
        {
            File.WriteAllText("history.json", "[]");
        }
        catch (Exception ex)
        {
        }
    }

    public async Task AddNewHistory(string description)
    {
        await FileLock.WaitAsync();
        try
        {
            var json = File.Exists("history.json") ? await File.ReadAllTextAsync("history.json") : "[]";
            var history = JsonSerializer.Deserialize<List<History>>(json) ?? new List<History>();

            var newHistory = new History(DateTime.Now, description);
            history.Add(newHistory);

            var newJson = JsonSerializer.Serialize(history, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await File.WriteAllTextAsync("history.json", newJson);
        }
        finally
        {
            FileLock.Release();
        }
    }
}
