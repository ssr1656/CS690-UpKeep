using System.Text.Json;
using UpKeep.Models;

namespace UpKeep.Services;

public class DataStore
{
    private readonly string _dataFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private UpKeepData _data = new();

    public DataStore(string? dataFilePath = null)
    {
        _dataFilePath = dataFilePath ?? Path.Combine(AppContext.BaseDirectory, "upkeep_data.json");
    }

    public List<Asset> GetAllAssets() => _data.Assets;
    public IReadOnlyList<MaintenanceLog> Logs => _data.Logs.AsReadOnly();

    public void Load()
    {
        if (!File.Exists(_dataFilePath))
        {
            _data = new UpKeepData();
            return;
        }

        var json = File.ReadAllText(_dataFilePath);
        _data = JsonSerializer.Deserialize<UpKeepData>(json, JsonOptions) ?? new UpKeepData();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_data, JsonOptions);
        File.WriteAllText(_dataFilePath, json);
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

    // FR-1.2: Delete an asset and all its associated logs
    public bool DeleteAsset(Guid assetId)
    {
        var asset = _data.Assets.FirstOrDefault(a => a.Id == assetId);
        if (asset == null) return false;

        _data.Assets.Remove(asset);
        _data.Logs.RemoveAll(l => l.AssetId == assetId);
        Save();
        return true;
    }

    // FR-4.1: Update asset frequency
    public void UpdateAssetFrequency(Guid assetId, int? frequencyInDays)
    {
        var asset = _data.Assets.FirstOrDefault(a => a.Id == assetId);
        if (asset != null)
        {
            asset.FrequencyInDays = frequencyInDays;
            Save();
        }
    }
}
