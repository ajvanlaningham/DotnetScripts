using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Implementations;
using ShopifySharp.GraphQL;

namespace GetProductDescription
{
    internal class Program
    {
        private static ConfigurationService _configurationService;
        private static ShopifySharp.ProductService _productService;

        static async Task Main(string[] args)
        {
            _configurationService = new ConfigurationService("appsettings.json");

            ShopifyAdminAPISettings apiSettings = _configurationService.GetShopifySettings();
            _productService = new ShopifySharp.ProductService(apiSettings.StoreUrl, apiSettings.AccessToken);

            ShopifySharp.Product product = await _productService.GetAsync(8331143741590);

            Console.WriteLine(product.BodyHtml);
        }
    }
}
