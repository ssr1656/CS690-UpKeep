using UpKeep.Models;
using UpKeep.Services;

namespace UpKeep.Tests.Services;

public class DataStoreTests : IDisposable
{
    private readonly string _testDataFile;
    private readonly DataStore _dataStore;

    public DataStoreTests()
    {
        _testDataFile = Path.Combine(Path.GetTempPath(), $"test_upkeep_{Guid.NewGuid()}.json");
        _dataStore = new DataStore(_testDataFile);
    }

    public void Dispose()
    {
        if (File.Exists(_testDataFile))
        {
            File.Delete(_testDataFile);
        }
    }

    [Fact]
    public void DataStore_Load_InitializesEmptyDataIfFileNotExists()
    {
        Assert.False(File.Exists(_testDataFile));
        _dataStore.Load();
        
        var assets = _dataStore.GetAllAssets();
        Assert.NotNull(assets);
        Assert.Empty(assets);
    }

    [Fact]
    public void DataStore_AddAsset_ShouldAddAndPersist()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "Test Asset", Location = "Building A" };
        
        _dataStore.AddAsset(asset);
        var assets = _dataStore.GetAllAssets();
        
        Assert.Single(assets);
        Assert.Equal("Test Asset", assets[0].Name);
        Assert.Equal("Building A", assets[0].Location);
    }

    [Fact]
    public void DataStore_AddAsset_ShouldPersistToFile()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "Persistent Asset", Location = "Room 101" };
        _dataStore.AddAsset(asset);
        
        var newDataStore = new DataStore(_testDataFile);
        newDataStore.Load();
        var assets = newDataStore.GetAllAssets();
        
        Assert.Single(assets);
        Assert.Equal("Persistent Asset", assets[0].Name);
    }

    [Fact]
    public void DataStore_GetAllAssets_ReturnsEmptyListInitially()
    {
        _dataStore.Load();
        var assets = _dataStore.GetAllAssets();
        
        Assert.NotNull(assets);
        Assert.Empty(assets);
    }

    [Fact]
    public void DataStore_AddLog_ShouldAddAndPersist()
    {
        _dataStore.Load();
        var assetId = Guid.NewGuid();
        var log = new MaintenanceLog
        {
            AssetId = assetId,
            Date = DateTime.Now,
            Description = "Test maintenance",
            Cost = 100m
        };
        
        _dataStore.AddLog(log);
        var logs = _dataStore.GetLogsForAsset(assetId);
        
        Assert.Single(logs);
        Assert.Equal("Test maintenance", logs[0].Description);
        Assert.Equal(100m, logs[0].Cost);
    }

    [Fact]
    public void DataStore_GetLogsForAsset_ReturnsOnlyMatchingLogs()
    {
        _dataStore.Load();
        var assetId1 = Guid.NewGuid();
        var assetId2 = Guid.NewGuid();
        
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId1, Description = "Log 1" });
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId1, Description = "Log 2" });
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId2, Description = "Log 3" });
        
        var logsForAsset1 = _dataStore.GetLogsForAsset(assetId1);
        var logsForAsset2 = _dataStore.GetLogsForAsset(assetId2);
        
        Assert.Equal(2, logsForAsset1.Count);
        Assert.Single(logsForAsset2);
    }

    [Fact]
    public void DataStore_GetLogsForAsset_ReturnsEmptyListForNonExistentAsset()
    {
        _dataStore.Load();
        var logs = _dataStore.GetLogsForAsset(Guid.NewGuid());
        
        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    [Fact]
    public void DataStore_GetLastServiceDate_ReturnsNullWhenNoLogs()
    {
        _dataStore.Load();
        var assetId = Guid.NewGuid();
        
        var lastDate = _dataStore.GetLastServiceDate(assetId);
        
        Assert.Null(lastDate);
    }

    [Fact]
    public void DataStore_GetLastServiceDate_ReturnsMostRecentDate()
    {
        _dataStore.Load();
        var assetId = Guid.NewGuid();
        var date1 = new DateTime(2024, 1, 1);
        var date2 = new DateTime(2024, 3, 15);
        var date3 = new DateTime(2024, 2, 10);
        
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId, Date = date1 });
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId, Date = date2 });
        _dataStore.AddLog(new MaintenanceLog { AssetId = assetId, Date = date3 });
        
        var lastDate = _dataStore.GetLastServiceDate(assetId);
        
        Assert.Equal(date2, lastDate);
    }

    [Fact]
    public void DataStore_DeleteAsset_RemovesAssetAndLogs()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "To Delete", Location = "Building B" };
        _dataStore.AddAsset(asset);
        _dataStore.AddLog(new MaintenanceLog { AssetId = asset.Id, Description = "Log to delete" });
        
        _dataStore.DeleteAsset(asset.Id);
        
        var assets = _dataStore.GetAllAssets();
        var logs = _dataStore.GetLogsForAsset(asset.Id);
        
        Assert.Empty(assets);
        Assert.Empty(logs);
    }

    [Fact]
    public void DataStore_UpdateAssetFrequency_ShouldUpdateFrequency()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "Test Asset", Location = "Building A", FrequencyInDays = null };
        _dataStore.AddAsset(asset);
        
        _dataStore.UpdateAssetFrequency(asset.Id, 30);
        
        var assets = _dataStore.GetAllAssets();
        Assert.Equal(30, assets[0].FrequencyInDays);
    }

    [Fact]
    public void DataStore_UpdateAssetFrequency_CanSetToNull()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "Test Asset", Location = "Building A", FrequencyInDays = 30 };
        _dataStore.AddAsset(asset);
        
        _dataStore.UpdateAssetFrequency(asset.Id, null);
        
        var assets = _dataStore.GetAllAssets();
        Assert.Null(assets[0].FrequencyInDays);
    }

    [Fact]
    public void DataStore_UpdateAssetFrequency_PersistsToFile()
    {
        _dataStore.Load();
        var asset = new Asset { Name = "Test Asset", Location = "Building A" };
        _dataStore.AddAsset(asset);
        _dataStore.UpdateAssetFrequency(asset.Id, 45);
        
        var newDataStore = new DataStore(_testDataFile);
        newDataStore.Load();
        var assets = newDataStore.GetAllAssets();
        
        Assert.Equal(45, assets[0].FrequencyInDays);
    }

    [Fact]
    public void DataStore_MultipleOperations_ShouldMaintainDataIntegrity()
    {
        _dataStore.Load();
        
        var asset1 = new Asset { Name = "Asset 1", Location = "Location 1" };
        var asset2 = new Asset { Name = "Asset 2", Location = "Location 2" };
        _dataStore.AddAsset(asset1);
        _dataStore.AddAsset(asset2);
        
        _dataStore.AddLog(new MaintenanceLog { AssetId = asset1.Id, Description = "Log 1", Cost = 100m });
        _dataStore.AddLog(new MaintenanceLog { AssetId = asset1.Id, Description = "Log 2", Cost = 200m });
        _dataStore.AddLog(new MaintenanceLog { AssetId = asset2.Id, Description = "Log 3", Cost = 150m });
        
        _dataStore.UpdateAssetFrequency(asset1.Id, 30);
        
        var assets = _dataStore.GetAllAssets();
        var logsAsset1 = _dataStore.GetLogsForAsset(asset1.Id);
        var logsAsset2 = _dataStore.GetLogsForAsset(asset2.Id);
        
        Assert.Equal(2, assets.Count);
        Assert.Equal(2, logsAsset1.Count);
        Assert.Single(logsAsset2);
        Assert.Equal(30, assets.First(a => a.Id == asset1.Id).FrequencyInDays);
    }
}
