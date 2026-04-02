using UpKeep.Models;

namespace UpKeep.Tests.Models;

public class AssetTests
{
    [Fact]
    public void Asset_Constructor_ShouldInitializeWithDefaultValues()
    {
        var asset = new Asset();
        
        Assert.NotEqual(Guid.Empty, asset.Id);
        Assert.NotNull(asset.Name);
        Assert.NotNull(asset.Location);
        Assert.Null(asset.FrequencyInDays);
    }

    [Fact]
    public void Asset_Properties_ShouldSetAndGetCorrectly()
    {
        var assetId = Guid.NewGuid();
        var asset = new Asset
        {
            Id = assetId,
            Name = "Test Asset",
            Location = "Building A",
            FrequencyInDays = 30
        };

        Assert.Equal(assetId, asset.Id);
        Assert.Equal("Test Asset", asset.Name);
        Assert.Equal("Building A", asset.Location);
        Assert.Equal(30, asset.FrequencyInDays);
    }

    [Fact]
    public void Asset_FrequencyInDays_CanBeNull()
    {
        var asset = new Asset
        {
            Name = "Test Asset",
            Location = "Building A",
            FrequencyInDays = null
        };

        Assert.Null(asset.FrequencyInDays);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(365)]
    public void Asset_FrequencyInDays_AcceptsValidValues(int frequency)
    {
        var asset = new Asset { FrequencyInDays = frequency };
        
        Assert.Equal(frequency, asset.FrequencyInDays);
    }
}
