using UpKeep.Services;

namespace UpKeep;

class Program
{
    static void Main(string[] args)
    {
        var store = new DataStore();
        var ui = new ConsoleUI(store);
        ui.Run();
    }
}
