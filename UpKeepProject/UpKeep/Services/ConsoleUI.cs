using UpKeep.Models;

namespace UpKeep.Services;

public class ConsoleUI
{
    private readonly DataStore _store;

    public ConsoleUI(DataStore store)
    {
        _store = store;
    }

    public void Run()
    {
        _store.Load();

        while (true)
        {
            Console.Clear();
            ShowDashboard();

            Console.WriteLine("========================================");
            Console.WriteLine("  1. Manage Home Assets");
            Console.WriteLine("  2. Log Maintenance Event");
            Console.WriteLine("  3. View Maintenance History");
            Console.WriteLine("  4. Exit");
            Console.WriteLine("========================================");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    ManageAssets();
                    break;
                case "2":
                    LogMaintenanceEvent();
                    break;
                case "3":
                    ViewMaintenanceHistory();
                    break;
                case "4":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    // ──────────────────────────────────────────────
    // FR-5.1 + FR-5.2: Dashboard with red overdue items
    // ──────────────────────────────────────────────
    private void ShowDashboard()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("         UpKeep - Home Maintenance      ");
        Console.WriteLine("========================================");
        Console.WriteLine("  Dashboard Overview");
        Console.WriteLine();

        var assets = _store.GetAllAssets();

        if (assets.Count == 0)
        {
            Console.WriteLine("  No assets yet. Add some to get started.");
            Console.WriteLine();
            return;
        }

        var today = DateTime.Today;
        var overdueItems = new List<(Asset Asset, DateTime? NextDue, DateTime? LastService)>();

        foreach (var asset in assets)
        {
            var lastService = _store.GetLastServiceDate(asset.Id);
            DateTime? nextDue = null;

            if (lastService.HasValue && asset.FrequencyInDays.HasValue)
            {
                nextDue = lastService.Value.AddDays(asset.FrequencyInDays.Value);
            }

            bool isOverdue = nextDue.HasValue && nextDue.Value.Date <= today;
            bool neverServiced = !lastService.HasValue;

            if (isOverdue || neverServiced)
            {
                overdueItems.Add((asset, nextDue, lastService));
            }
        }

        if (overdueItems.Count == 0)
        {
            Console.WriteLine("  All caught up! No overdue or pending tasks.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  {"Asset",-20} {"Location",-15} {"Last Service",-14} {"Next Due",-14} {"Status"}");
        Console.WriteLine("  " + new string('-', 75));

        foreach (var (asset, nextDue, lastService) in overdueItems)
        {
            var lastStr = lastService.HasValue
                ? lastService.Value.ToString("yyyy-MM-dd")
                : "Never";
            var nextStr = nextDue.HasValue
                ? nextDue.Value.ToString("yyyy-MM-dd")
                : "N/A";

            bool isOverdue = nextDue.HasValue && nextDue.Value.Date <= today;
            string status = isOverdue ? "OVERDUE" : "NEEDS SERVICE";

            // FR-5.2: Color-code overdue tasks in red
            if (isOverdue)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"  {asset.Name,-20} {asset.Location,-15} {lastStr,-14} {nextStr,-14} {status}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    // ──────────────────────────────────────────────
    // FR-1.1: Manage Home Assets – create and view
    // ──────────────────────────────────────────────
    private void ManageAssets()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========== Manage Home Assets ==========");
            Console.WriteLine("  1. View All Assets");
            Console.WriteLine("  2. Add New Asset");
            Console.WriteLine("  3. Delete Asset");
            Console.WriteLine("  4. Back to Main Menu");
            Console.WriteLine("========================================");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    ViewAllAssets();
                    break;
                case "2":
                    AddNewAsset();
                    break;
                case "3":
                    DeleteAsset();
                    break;
                case "4":
                    return;
            }
        }
    }

    private void ViewAllAssets()
    {
        Console.Clear();
        Console.WriteLine("========== All Assets ==========");
        Console.WriteLine();

        var assets = _store.GetAllAssets();

        if (assets.Count == 0)
        {
            Console.WriteLine("No assets found.");
            Pause();
            return;
        }

        Console.WriteLine($"{"#",-4} {"Name",-25} {"Location",-20} {"ID"}");
        Console.WriteLine(new string('-', 75));

        for (int i = 0; i < assets.Count; i++)
        {
            var a = assets[i];
            Console.WriteLine($"{i + 1,-4} {a.Name,-25} {a.Location,-20} {a.Id}");
        }

        Console.WriteLine();
        Pause();
    }

    private void AddNewAsset()
    {
        Console.Clear();
        Console.WriteLine("========== Add New Asset ==========");
        Console.WriteLine();

        Console.Write("Asset Name: ");
        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be empty.");
            Pause();
            return;
        }

        Console.Write("Location: ");
        var location = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Maintenance frequency in days (optional, press Enter to skip): ");
        var freqInput = Console.ReadLine()?.Trim();
        int? frequency = null;

        if (!string.IsNullOrWhiteSpace(freqInput) && int.TryParse(freqInput, out int freq) && freq > 0)
        {
            frequency = freq;
        }

        var asset = new Asset
        {
            Name = name,
            Location = location,
            FrequencyInDays = frequency
        };

        _store.AddAsset(asset);

        Console.WriteLine();
        Console.WriteLine($"Asset '{asset.Name}' added successfully (ID: {asset.Id}).");
        Pause();
    }

    // ──────────────────────────────────────────────
    // FR-2.1: Log Maintenance Event
    // ──────────────────────────────────────────────
    private void LogMaintenanceEvent()
    {
        Console.Clear();
        Console.WriteLine("========== Log Maintenance Event ==========");
        Console.WriteLine();

        var asset = SelectAsset();
        if (asset == null) return;

        Console.Write("Date of service (yyyy-MM-dd) [default: today]: ");
        var dateInput = Console.ReadLine()?.Trim();
        DateTime date;

        if (string.IsNullOrWhiteSpace(dateInput))
        {
            date = DateTime.Today;
        }
        else if (!DateTime.TryParse(dateInput, out date))
        {
            Console.WriteLine("Invalid date format.");
            Pause();
            return;
        }

        // FR-2.2: Validate that a maintenance date cannot be in the future
        if (date.Date > DateTime.Today)
        {
            Console.WriteLine("Error: Maintenance date cannot be in the future.");
            Pause();
            return;
        }

        Console.Write("Description of work: ");
        var description = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("Description cannot be empty.");
            Pause();
            return;
        }

        Console.Write("Cost ($): ");
        var costInput = Console.ReadLine()?.Trim();

        if (!decimal.TryParse(costInput, out decimal cost) || cost < 0)
        {
            Console.WriteLine("Invalid cost value.");
            Pause();
            return;
        }

        var log = new MaintenanceLog
        {
            AssetId = asset.Id,
            Date = date,
            Description = description,
            Cost = cost
        };

        _store.AddLog(log);

        Console.WriteLine();
        Console.WriteLine("Maintenance event logged successfully.");
        Pause();
    }

    // ──────────────────────────────────────────────
    // FR-3.1: View Maintenance History (most recent first)
    // ──────────────────────────────────────────────
    private void ViewMaintenanceHistory()
    {
        Console.Clear();
        Console.WriteLine("========== Maintenance History ==========");
        Console.WriteLine();

        var asset = SelectAsset();
        if (asset == null) return;

        var logs = _store.GetLogsForAsset(asset.Id);

        Console.Clear();
        Console.WriteLine($"========== History for: {asset.Name} ==========");
        Console.WriteLine();

        if (logs.Count == 0)
        {
            Console.WriteLine("No maintenance records found for this asset.");
            Pause();
            return;
        }

        Console.WriteLine($"{"Date",-14} {"Description",-35} {"Cost",10}");
        Console.WriteLine(new string('-', 62));

        foreach (var log in logs)
        {
            Console.WriteLine($"{log.Date:yyyy-MM-dd}     {log.Description,-35} {log.Cost,10:C}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total records: {logs.Count}");

        // FR-3.2: Calculate Days Since Last Service
        var lastServiceDate = logs[0].Date;
        var daysSince = (DateTime.Today - lastServiceDate.Date).Days;
        Console.WriteLine($"Days since last service: {daysSince}");

        Pause();
    }

    // ──────────────────────────────────────────────
    // FR-1.2: Delete asset and all associated history
    // ──────────────────────────────────────────────
    private void DeleteAsset()
    {
        Console.Clear();
        Console.WriteLine("========== Delete Asset ==========");
        Console.WriteLine();

        var asset = SelectAsset();
        if (asset == null) return;

        Console.Write($"Are you sure you want to delete '{asset.Name}' and all its history? (y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();

        if (confirm == "y")
        {
            _store.DeleteAsset(asset.Id);
            Console.WriteLine($"Asset '{asset.Name}' and all associated logs deleted.");
        }
        else
        {
            Console.WriteLine("Deletion cancelled.");
        }

        Pause();
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────
    private Asset? SelectAsset()
    {
        var assets = _store.GetAllAssets();

        if (assets.Count == 0)
        {
            Console.WriteLine("No assets found. Add assets first.");
            Pause();
            return null;
        }

        Console.WriteLine("Select an asset:");
        for (int i = 0; i < assets.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {assets[i].Name} ({assets[i].Location})");
        }

        Console.Write("Enter number: ");
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out int index) || index < 1 || index > assets.Count)
        {
            Console.WriteLine("Invalid selection.");
            Pause();
            return null;
        }

        return assets[index - 1];
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}
