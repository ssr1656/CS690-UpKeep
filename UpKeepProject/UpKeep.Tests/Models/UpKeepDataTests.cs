using UpKeep.Models;

namespace UpKeep.Tests.Models;

public class UpKeepDataTests
{
    [Fact]
    public void UpKeepData_Constructor_ShouldInitializeEmptyLists()
    {
        var data = new UpKeepData();
        
        Assert.NotNull(data.Assets);
        Assert.NotNull(data.Logs);
        Assert.Empty(data.Assets);
        Assert.Empty(data.Logs);
    }

    [Fact]
    public void UpKeepData_Assets_CanAddAndRetrieve()
    {
        var data = new UpKeepData();
        var asset = new Asset { Name = "Test Asset", Location = "Building A" };
        
        data.Assets.Add(asset);
        
        Assert.Single(data.Assets);
        Assert.Equal("Test Asset", data.Assets[0].Name);
    }

    [Fact]
    public void UpKeepData_MaintenanceLogs_CanAddAndRetrieve()
    {
        var data = new UpKeepData();
        var log = new MaintenanceLog 
        { 
            AssetId = Guid.NewGuid(), 
            Description = "Test maintenance",
            Cost = 100m
        };
        
        data.Logs.Add(log);
        
        Assert.Single(data.Logs);
        Assert.Equal("Test maintenance", data.Logs[0].Description);
    }

    [Fact]
    public void UpKeepData_CanContainMultipleAssetsAndLogs()
    {
        var data = new UpKeepData();
        
        data.Assets.Add(new Asset { Name = "Asset 1" });
        data.Assets.Add(new Asset { Name = "Asset 2" });
        data.Assets.Add(new Asset { Name = "Asset 3" });
        
        data.Logs.Add(new MaintenanceLog { Description = "Log 1" });
        data.Logs.Add(new MaintenanceLog { Description = "Log 2" });
        
        Assert.Equal(3, data.Assets.Count);
        Assert.Equal(2, data.Logs.Count);
    }
}
