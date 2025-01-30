using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Classes.GQLObjects;
using Newtonsoft.Json;

namespace FinancialConsolidationReport.cs
{
    internal class Program
    {
        private static FinancialReportService _service;
        private static CustomOrderService _customOrderService;
        static async Task Main(string[] args)
        {
            var configService = new ConfigurationService("appsettings.json");
            var excelWriter = new ExcelWriterService();

            ShopifyAdminAPISettings shopifySettings = configService.GetShopifySettings();
            _service = new FinancialReportService(shopifySettings);
            _customOrderService = new CustomOrderService(shopifySettings);

            //List<OrderTransaction> transactions = await _service.RunFinancialConsolidationReport();
            var payouts = await _customOrderService.FetchAllPayoutsAsync();

            string json = JsonConvert.SerializeObject(payouts);

            Console.WriteLine(json);
            Console.ReadLine();

            
        }
    }
}
