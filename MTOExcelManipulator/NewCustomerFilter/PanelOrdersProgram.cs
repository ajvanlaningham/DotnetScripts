using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;

public static class PanelOrdersProgram
{
    public static async Task RunAsync(ShopifyAdminAPISettings settings, IExcelWriterService excelWriter)
    {
        var configService = new ConfigurationService(); // default to "appsettings.json"
        string reportDirectory = configService.GetValue("FilePaths:ReportDirectory");

        var customCustomerService = new CustomCustomerService(settings);
        var orderService = new CustomOrderService(settings);

        Console.WriteLine("Fetching all customers...");
        List<ShopifySharp.Customer> allCustomers = await customCustomerService.FetchAllCustomersAsync();
        Console.WriteLine($"Total customers fetched: {allCustomers.Count}");

        List<CustomerPanelInfo> panelOnlyCustomers = new List<CustomerPanelInfo>();

        foreach (var customer in allCustomers)
        {
            string gid = $"gid://shopify/Customer/{customer.Id}";
            List<OrdersByCustomerResponse.OrderDetail> orders = await orderService.GetOrdersByCustomerIdAsync(gid);

            if (orders.Any() && orders.All(o => o.LineItems.All(li => li.Product.Tags.Contains("panel"))))
            {
                panelOnlyCustomers.Add(new CustomerPanelInfo
                {
                    Email = customer.Email,
                    CreatedAt = customer.CreatedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                    TotalSpent = customer.TotalSpent.Value,
                    IPAddress = orders.FirstOrDefault()?.ClientIp ?? "N/A"
                });

                Console.WriteLine($"- {customer.Email} (IP Address: {orders.FirstOrDefault()?.ClientIp ?? "N/A"})");
            }
        }

        string filePath = Path.Combine(reportDirectory, "PanelOnlyOrders.xlsx");
        excelWriter.WriteExcelFile(filePath, panelOnlyCustomers, "Panel Orders");

        Console.WriteLine($"Exported {panelOnlyCustomers.Count} customers who only ordered panels to {filePath}");
    }

    private class CustomerPanelInfo
    {
        public string Email { get; set; }
        public string CreatedAt { get; set; }
        public decimal TotalSpent { get; set; }
        public string IPAddress { get; set; }
    }
}
