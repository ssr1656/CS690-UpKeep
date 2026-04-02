using UpKeep.Models;

namespace UpKeep.Tests.Models;

public class MaintenanceLogTests
{
    [Fact]
    public void MaintenanceLog_Constructor_ShouldInitializeWithDefaultValues()
    {
        var log = new MaintenanceLog();
        
        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(Guid.Empty, log.AssetId);
        Assert.Equal(default(DateTime), log.Date);
        Assert.NotNull(log.Description);
        Assert.Equal(0, log.Cost);
    }

    [Fact]
    public void MaintenanceLog_Properties_ShouldSetAndGetCorrectly()
    {
        var logId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var date = new DateTime(2024, 3, 15);
        
        var log = new MaintenanceLog
        {
            Id = logId,
            AssetId = assetId,
            Date = date,
            Description = "Oil change",
            Cost = 150.50m
        };

        Assert.Equal(logId, log.Id);
        Assert.Equal(assetId, log.AssetId);
        Assert.Equal(date, log.Date);
        Assert.Equal("Oil change", log.Description);
        Assert.Equal(150.50m, log.Cost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(1000.99)]
    public void MaintenanceLog_Cost_AcceptsValidValues(decimal cost)
    {
        var log = new MaintenanceLog { Cost = cost };
        
        Assert.Equal(cost, log.Cost);
    }

    [Fact]
    public void MaintenanceLog_Date_CanBeSetToAnyDateTime()
    {
        var pastDate = new DateTime(2020, 1, 1);
        var futureDate = new DateTime(2030, 12, 31);
        
        var log1 = new MaintenanceLog { Date = pastDate };
        var log2 = new MaintenanceLog { Date = futureDate };
        
        Assert.Equal(pastDate, log1.Date);
        Assert.Equal(futureDate, log2.Date);
    }
}
