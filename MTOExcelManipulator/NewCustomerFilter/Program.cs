using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;

internal class Program
{
    static async Task Main(string[] args)
    {
        var configService = new ConfigurationService("appsettings.json");
        var excelWriter = new ExcelWriterService(); 
        ShopifyAdminAPISettings shopifySettings = configService.GetShopifySettings();

        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Identify potential spam accounts (Excel export)");
        Console.WriteLine("2. List customers who only ordered panels and their IPs (Excel export)");
        Console.Write("Enter your choice (1 or 2): ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                await SpamFilterProgram.RunAsync(shopifySettings, excelWriter);
                break;
            case "2":
                await PanelOrdersProgram.RunAsync(shopifySettings, excelWriter);
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting.");
                break;
        }

        Console.WriteLine("Done.");
    }
}
