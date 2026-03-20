namespace UpKeep.Models;

public class UpKeepData
{
    public List<Asset> Assets { get; set; } = new();
    public List<MaintenanceLog> Logs { get; set; } = new();
}
