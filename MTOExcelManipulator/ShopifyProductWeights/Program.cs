using ClassLibrary.Classes;
using ClassLibrary.Services.Implemetations;

namespace ShopifyProductWeights
{
    internal class Program
    {
        private static CustomProductService _productService;
        static async Task Main(string[] args)
        {
            var configService = new ConfigurationService("appsettings.json");
            ShopifyAdminAPISettings shopifySettings = configService.GetShopifySettings();

            Console.WriteLine($"Using Shopify Store: {shopifySettings.StoreUrl}");

            _productService = new CustomProductService(shopifySettings);
            var products = await _productService.FetchAllProductsAsync();


            //learn sumthin' new every day! LINQ to the rescue
            // Filter only non-archived products (Status should be "active" or "draft")
            var nonArchivedProducts = products
                .Where(p => p.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Filtered {nonArchivedProducts.Count} non-archived products.");

            // Collect all variants with a weight of 0 from non-archived products
            var zeroWeightVariants = nonArchivedProducts
                .SelectMany(p => p.Variants)
                .Where(v => v.Weight.HasValue && v.Weight.Value == 0 && v.Title != "Pounds")
                .ToList();

            Console.WriteLine($"Found {zeroWeightVariants.Count} variants with weight 0:");

            foreach (var variant in zeroWeightVariants)
            {
                Console.WriteLine($"Product ID: {variant.ProductId}, Variant ID: {variant.Id}, SKU: {variant.SKU}, Title: {variant.Title}, Weight: {variant.Weight}");
            }
        }
    }
}
