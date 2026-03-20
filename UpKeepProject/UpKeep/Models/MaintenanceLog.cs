namespace UpKeep.Models;

public class MaintenanceLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
}
