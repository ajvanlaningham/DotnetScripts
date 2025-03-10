using ClassLibrary.Services.Implementations;
using System.Text.Json.Nodes;

namespace GetOrderString
{
    internal class Program
    {
        static CustomCompanyService _companyService;
        static async Task Main(string[] args)
        {
            var service = new CustomOrderService(new ConfigurationService("appsettings.json").GetShopifySettings());
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _companyService = new CustomCompanyService(config.GetShopifySettings());

            Console.Write("What Order? ");
            long orderID = long.Parse(Console.ReadLine());
            ShopifySharp.Order order = await service.FetchOrderAsync(orderID);

            string siteID = string.Empty;
            
            if (order.Company is not  null)
            {
                long CompanyId = order.Company.Id.Value;
                long LocationId = order.Company.LocationId.Value;
                siteID = await _companyService.GetSiteID(CompanyId, LocationId);
            }

            Console.WriteLine(siteID);
        }
    }
}
