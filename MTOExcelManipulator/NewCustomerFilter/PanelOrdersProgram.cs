using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;

public static class PanelOrdersProgram
{
    public static async Task RunAsync(ShopifyAdminAPISettings settings, IExcelWriterService excelWriter)
    {
        var configService = new ConfigurationService();
        string reportDirectory = configService.GetValue("FilePaths:ReportDirectory");

        var customCustomerService = new CustomCustomerService(settings);
        var orderService = new CustomOrderService(settings);

        Console.WriteLine("Fetching all customers...");
        List<ShopifySharp.Customer> allCustomers = await customCustomerService.FetchAllCustomersAsync();
        Console.WriteLine($"Total customers fetched: {allCustomers.Count}");

        List<CustomerPanelInfo> panelOnlyCustomers = new List<CustomerPanelInfo>();
        var panelsByIpAddress = new Dictionary<string, int>();

        var customersWithNoSpend = allCustomers
            .Where(customer => customer.TotalSpent == 0) // Only customers who haven't spent money
            .ToList();

        foreach (var customer in customersWithNoSpend)
        {
            if (customer.Email.Contains("@ppg.com", StringComparison.OrdinalIgnoreCase)) continue;  // Skip PPG customers

            string gid = $"gid://shopify/Customer/{customer.Id}";
            List<OrdersByCustomerResponse.OrderDetail> orders = await orderService.GetOrdersByCustomerIdAsync(gid);

            int totalPanelsOrdered = 0;
            foreach (var order in orders)
            {
                var ipAddress = order.ClientIp ?? "Unknown IP";
                int panelCountInOrder = order.LineItems.Edges
                    .Where(li => li.Node.Product?.Tags.Contains("panel") ?? false)
                    .Sum(li => li.Node.Quantity);

                totalPanelsOrdered += panelCountInOrder;

                if (panelCountInOrder > 0)
                {
                    if (!panelsByIpAddress.ContainsKey(ipAddress))
                    {
                        panelsByIpAddress[ipAddress] = 0;
                    }
                    panelsByIpAddress[ipAddress] += panelCountInOrder;  // Sum panels for this IP
                }
            }

            if (totalPanelsOrdered > 0)
            {
                panelOnlyCustomers.Add(new CustomerPanelInfo
                {
                    ShopifyCustomerId = customer.Id.Value,
                    Email = customer.Email,
                    CreatedAt = customer.CreatedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                    TotalSpent = customer.TotalSpent ?? 0,
                    IPAddress = orders.FirstOrDefault()?.ClientIp ?? "N/A",
                    TotalPanelsOrdered = totalPanelsOrdered 
                });

                Console.WriteLine($"- {customer.Email} ordered {totalPanelsOrdered} panels (IP: {orders.FirstOrDefault()?.ClientIp ?? "N/A"})");
            }
        }

  
        Console.WriteLine("Total panels ordered by IP address:");
        foreach (var kvp in panelsByIpAddress)
        {
            Console.WriteLine($"IP Address: {kvp.Key}, Total Panels: {kvp.Value}");
        }

        string filePath = Path.Combine(reportDirectory, "PanelByIP.xlsx");

        excelWriter.WriteExcelFile(filePath, panelOnlyCustomers, "Panel Orders by Customer");
        excelWriter.WriteExcelFile(filePath, panelsByIpAddress.Select(ip => new { IPAddress = ip.Key, TotalPanels = ip.Value }).ToList(), "Total Panels by IP");

        Console.WriteLine($"Exported {panelOnlyCustomers.Count} customers who only ordered panels to {filePath}");
    }

    private class CustomerPanelInfo
    {
        public long ShopifyCustomerId { get; set; }
        public string Email { get; set; }
        public string CreatedAt { get; set; }
        public decimal TotalSpent { get; set; }
        public string IPAddress { get; set; }
        public int TotalPanelsOrdered { get; set; }
    }

}
