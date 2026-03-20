namespace UpKeep.Models;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? FrequencyInDays { get; set; }
}
