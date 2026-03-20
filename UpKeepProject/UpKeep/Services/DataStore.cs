using System.Text.Json;
using UpKeep.Models;

namespace UpKeep.Services;

public class DataStore
{
    private static readonly string DataFilePath =
        Path.Combine(AppContext.BaseDirectory, "upkeep_data.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private UpKeepData _data = new();

    public IReadOnlyList<Asset> Assets => _data.Assets.AsReadOnly();
    public IReadOnlyList<MaintenanceLog> Logs => _data.Logs.AsReadOnly();

    public void Load()
    {
        if (!File.Exists(DataFilePath))
        {
            _data = new UpKeepData();
            return;
        }

        var json = File.ReadAllText(DataFilePath);
        _data = JsonSerializer.Deserialize<UpKeepData>(json, JsonOptions) ?? new UpKeepData();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_data, JsonOptions);
        File.WriteAllText(DataFilePath, json);
    }

    // FR-1.1: Add a new asset
    public void AddAsset(Asset asset)
    {
        _data.Assets.Add(asset);
        Save();
    }

    // FR-2.1: Log a maintenance event
    public void AddLog(MaintenanceLog log)
    {
        _data.Logs.Add(log);
        Save();
    }

    // FR-3.1: Get logs for an asset, sorted most recent first
    public List<MaintenanceLog> GetLogsForAsset(Guid assetId)
    {
        return _data.Logs
            .Where(l => l.AssetId == assetId)
            .OrderByDescending(l => l.Date)
            .ToList();
    }

    // Get the most recent log date for an asset (used by dashboard)
    public DateTime? GetLastServiceDate(Guid assetId)
    {
        return _data.Logs
            .Where(l => l.AssetId == assetId)
            .OrderByDescending(l => l.Date)
            .Select(l => (DateTime?)l.Date)
            .FirstOrDefault();
    }
}
