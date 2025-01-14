using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;

public static class SpamFilterProgram
{
    public static async Task RunAsync(ShopifyAdminAPISettings settings, IExcelWriterService excelWriter)
    {
        var configService = new ConfigurationService(); // default to "appsettings.json"
        string reportDirectory = configService.GetValue("FilePaths:ReportDirectory");

        var customCustomerService = new CustomCustomerService(settings);

        try
        {
            Console.WriteLine("Fetching all customers...");
            List<ShopifySharp.Customer> allCustomers = await customCustomerService.FetchAllCustomersAsync();
            Console.WriteLine($"Total customers fetched: {allCustomers.Count}");

            DateTime lastWeek = DateTime.UtcNow.AddDays(-7);
            List<string> disposableEmailDomains = new List<string>
            {
                "mailinator.com", "guerrillamail.com", "10minutemail.com", "sharklasers.com"
            };

            List<ShopifySharp.Customer> potentialSpamAccounts = allCustomers
                .Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value >= lastWeek)
                .Where(c => c.TotalSpent == 0)
                .Where(c => !string.IsNullOrEmpty(c.Email) && !c.Email.EndsWith("@ppg.com", StringComparison.OrdinalIgnoreCase))
                .Where(c => !string.IsNullOrEmpty(c.Email) && !disposableEmailDomains.Any(domain => c.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
                .Where(c => !HasTooManySpecialCharacters(c.Email) && c.Email.Length < 50)
                .ToList();

            Console.WriteLine($"Potential spam accounts created in the last week: {potentialSpamAccounts.Count}");

            string filePath = Path.Combine(reportDirectory, "SpamAccounts.xlsx");
            excelWriter.WriteExcelFile(filePath, potentialSpamAccounts, "Spam Accounts");

            Console.WriteLine($"Exported {potentialSpamAccounts.Count} spam accounts to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static bool HasTooManySpecialCharacters(string email)
    {
        int specialCharacterCount = email.Count(c => !char.IsLetterOrDigit(c) && c != '@' && c != '.');
        return specialCharacterCount > 3;
    }
}
