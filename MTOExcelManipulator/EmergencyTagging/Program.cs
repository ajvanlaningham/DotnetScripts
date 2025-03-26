using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;
using System.Text.Json;

namespace EmergencyTagging
{
 

    public class Program
    {
        public static CustomProductService _productService;
        public static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _productService = new CustomProductService(config.GetShopifySettings());

            string filePath = config.GetValue("FilePaths:ProductsJson");
            string json = File.ReadAllText(filePath);

            List<Product> products = JsonSerializer.Deserialize<List<Product>>(json);
            Console.WriteLine($"Loaded {products.Count} products from JSON.");

            List<ShopifySharp.Product> liveProducts = await _productService.FetchAllProductsAsync();
            Console.WriteLine($"Fetched {liveProducts.Count} live products from the website.");

            var liveProductIds = new HashSet<long>(liveProducts.Select(lp => lp.Id.Value));

            var productsOnWebsite = products.Where(p => liveProductIds.Contains(p.ShopifyId)).ToList();
            Console.WriteLine($"Found {productsOnWebsite.Count} products already on the website.");

        }
    }

    public class Product
    {
        public long ShopifyId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Vendor { get; set; }
        public string ProductType { get; set; }
        public string Handle { get; set; }
        public string Tags { get; set; }
        public string Status { get; set; }
        public List<Variant> Variants { get; set; }
        public List<Option> Options { get; set; }
        public int Id { get; set; }
    }

    public class Variant
    {
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        public string Title { get; set; }
        public string SKU { get; set; }
        public int Position { get; set; }
        public double? Grams { get; set; }
        public string InventoryPolicy { get; set; }
        public string FulfillmentService { get; set; }
        public long? InventoryItemId { get; set; }
        public string InventoryManagement { get; set; }
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Barcode { get; set; }
        public int InventoryQuantity { get; set; }
        public double? Weight { get; set; }
        public string WeightUnit { get; set; }
    }

    public class Option
    {
        public long OptionId { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public List<string> Values { get; set; }
    }

}
