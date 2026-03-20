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
    // FR-5.1: Dashboard – placeholder (to be implemented)
    // ──────────────────────────────────────────────
    private void ShowDashboard()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("         UpKeep - Home Maintenance      ");
        Console.WriteLine("========================================");
        Console.WriteLine("  Dashboard: Coming Soon");
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
            Console.WriteLine("  3. Back to Main Menu");
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
                    return;
            }
        }
    }

    private void ViewAllAssets()
    {
        Console.Clear();
        Console.WriteLine("========== All Assets ==========");
        Console.WriteLine();

        var assets = _store.Assets;

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
        Pause();
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────
    private Asset? SelectAsset()
    {
        var assets = _store.Assets;

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
