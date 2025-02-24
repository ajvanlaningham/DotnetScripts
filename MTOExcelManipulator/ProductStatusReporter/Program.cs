using ClassLibrary.Services.Implementations;
using ShopifySharp;

namespace ProductStatusReporter
{
    internal class Program
    {
        private static ExcelWriterService _excelWriterService;
        private static CustomProductService _prodService;
        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelWriterService = new ExcelWriterService();
            _prodService = new CustomProductService(config.GetShopifySettings());
            string filePath = config.GetValue("FilePaths:ProductStatusReport");
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Console.WriteLine("Fetching all products...");
            List<Product> prodList = await _prodService.FetchAllProductsAsync();
            List<ProductActiveStatusClass> productActiveStatuses = new List<ProductActiveStatusClass>();
            foreach (Product prod in prodList)
            {
                ProductActiveStatusClass productActiveStatus = new ProductActiveStatusClass()
                {
                    ShopifyID = prod.Id.Value.ToString(),
                    Name = prod.Title,
                    SKU = prod.Variants.Any()? prod.Variants.FirstOrDefault()?.SKU : "",
                    Status = prod.Status,
                    ProductType = GetProductType(prod),
                    LinkToProduct = $"https://admin.shopify.com/store/ppg-powder-coatings/products/{prod.Id.Value.ToString()}"

                };
                productActiveStatuses.Add(productActiveStatus);

            }

            _excelWriterService.WriteExcelFile(filePath, productActiveStatuses);


        }

        private static string GetProductType(Product prod)
        {
            if (prod.Tags.Any())
            {
                if (prod.Tags.Contains("STOCKProduct"))
                {
                    return "Stock Product";
                }
                else if (prod.Tags.Contains("MTOProduct"))
                {
                    return "MTO Product";
                }
                else if (prod.Handle.Contains("Panel", StringComparison.OrdinalIgnoreCase))
                {
                    return "Panel";
                }
                else if (prod.Tags.Contains("Collection_Sale") | prod.Tags.Contains("Firesale_Creation") | prod.Title.Contains("FIRESALE", StringComparison.OrdinalIgnoreCase))
                {
                    return "Probably firesale";
                }
                else if (prod.Handle.Contains("Small Batch", StringComparison.OrdinalIgnoreCase) | prod.Tags.Contains("SampleProduct", StringComparison.OrdinalIgnoreCase))
                {
                    return "Small Batch";
                }
            }
            return "No Idea. Some other thing. What am I, a Wizard??";

        }
    }

    public class ProductActiveStatusClass
    {
        public string ShopifyID { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Status { get; set; }
        public string ProductType { get; set; }
        public string LinkToProduct { get; set; }
    }
}
