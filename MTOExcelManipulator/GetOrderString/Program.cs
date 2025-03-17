using ClassLibrary.Services.Implementations;
using System.Text.Json.Nodes;
using static ClassLibrary.Classes.GQLObjects.OrderByIDResponse;

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
            string orderID = Console.ReadLine();
            OrderNode order = await service.GetOrderByIdAsync($"gid://shopify/Order/{orderID}");

            Console.WriteLine(order.Name);

        }
    }

}
