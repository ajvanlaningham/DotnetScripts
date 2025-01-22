using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ShopifySharp;
using ShopifySharp.Factories;
using System.ComponentModel;
using System.Globalization;

namespace ProductRelabelingScript
{
    internal class Program
    {
        private static ExcelReaderService _excelReaderService;
        private static ConfigurationService _configurationService;
        private static CustomProductService _productService;

        private static string fileLocation;

        static async Task Main(string[] args)
        {
            _configurationService = new ConfigurationService("appsettings.json");
            _excelReaderService = new ExcelReaderService();

            fileLocation = _configurationService.GetValue("FilePaths:StockExcelLocation");

            Console.WriteLine(fileLocation);
            List<StockListObject> stockList = _excelReaderService.ReadExcelFile<StockListObject>(fileLocation);


            Console.WriteLine($"Total valid stock items: {stockList.Count}");

            ShopifyAdminAPISettings apiSettings = _configurationService.GetShopifySettings();
            _productService = new CustomProductService(apiSettings);

            List<Product> updatedStockProduct = new List<Product>();
            List<StockListObject> unfound = new List<StockListObject>();

            foreach (StockListObject stock in stockList)
            {
                stock.SKU = stock.SKU?.Trim().Split('/').FirstOrDefault();

                List<ClassLibrary.Classes.GQLObjects.Product> products = await _productService.FindProductsBySkuAsync(stock.SKU);

                var filteredProducts = products
                    .Select(product => new ClassLibrary.Classes.GQLObjects.Product
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Tags = product.Tags,
                        Variants = product.Variants
                            .Where(variant => !variant.Sku.Contains("panel", StringComparison.OrdinalIgnoreCase) &&
                                              !variant.Sku.Contains("sb", StringComparison.OrdinalIgnoreCase))
                            .ToList()
                    })
                    .Where(product => product.Variants.Any()) // Only keep products with remaining variants
                    .ToList();


                if (!filteredProducts.Any())
                {
                    Console.WriteLine($"No valid variants found for SKU: {stock.SKU}, Product Name: {stock.ProductName}");
                    unfound.Add(stock);
                    continue;
                }

                var product = filteredProducts.First();
                stock.ProductID = long.TryParse(product.Id?.Split('/').Last(), out long parsedId) ? parsedId : (long?)null;
                product.Tags = UpdateStockTags(stock, product.Tags);
                string tagString = string.Join(",", product.Tags);

                Product newProduct = new Product()
                {
                    Id = stock.ProductID,
                    Tags = tagString
                };

                updatedStockProduct.Add(newProduct);

            }

            foreach (StockListObject stock in unfound)
            {
                Console.WriteLine($"Unfound StockList Product: {stock.SKU}");
            }

            
            await _productService.UpdateProductsTagsAsync(updatedStockProduct);

            Console.WriteLine("Process completed.");

        }

        private static List<string> UpdateStockTags(StockListObject stock, List<string> tags)
        {
            tags = UpdateMinimumFillQuantity(tags, stock);
            tags = RemoveMTOTags(tags);
            tags = AddStockProductTag(tags);
            //tags = AddMTOProductTag(tags);
            return tags;
        }

        private static List<string> AddMTOProductTag(List<string> tags)
        {
            string newTag = "MTOProduct";
            if (!tags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            {
                tags.Add(newTag);
            }
            return tags;
        }

        private static List<string> UpdateMinimumFillQuantity(List<string> tags, StockListObject stockObj)
        {
            int fillQuant = stockObj.FillQuantity.Value;
            if (fillQuant > 0)
            {
                tags.RemoveAll(t => t.StartsWith("Fill Quantity"));
                tags.Add($"Fill Quantity {fillQuant}");
            }

            int newMinQuant = stockObj.FillQuantity.Value;
            if (newMinQuant > 0)
            {
                tags.RemoveAll(t => t.StartsWith("Minimum Quantity", StringComparison.OrdinalIgnoreCase));
                tags.Add($"Minimum Quantity {newMinQuant}");
            }

            return tags;
        }

        private static List<string> AddStockProductTag(List<string> tags)
        {
            string newTag = "STOCKProduct";
            if (!tags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            {
                tags.Add(newTag);
            }
            return tags;
        }

        private static List<string> RemoveMTOTags(List<string> tags)
        {
            var tagsToRemove = tags.Where(tag => tag.Contains("MTO", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var tag in tagsToRemove)
            {
                tags.Remove(tag);
            }
            return tags;
        }
    }

    public class StockListObject
    {
        [ExcelReaderService.ExcelColumn("Product")]
        public string SKU { get; set;}

        [ExcelReaderService.ExcelColumn("Current Stock Status")]
        public string StockStatus { get; set; }

        [ExcelReaderService.ExcelColumn("PRODUCT NAME")]
        public string? ProductName { get; set; }

        [ExcelReaderService.ExcelColumn("Location")]
        public string? Location { get; set; }

        [ExcelReaderService.ExcelColumn("FILL QUANTITY")]
        public int? FillQuantity { get; set; }

        public long? ProductID { get; set; }
    }
}
