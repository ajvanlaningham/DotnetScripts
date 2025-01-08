using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;
using System.Text.Json;

namespace MTOExcelManipulator
{
    internal class Program
    {
        private static ExcelReaderService _readerService;
        private static ExcelWriterService _writerService;
        private static string fileLocation;
        private static string filePath;
        static void Main(string[] args)
        {

            var configService = new ConfigurationService();
            var fileSettings = configService.GetSection<FileSettings>("FileSettings");

            fileLocation = fileSettings.FileLocation;
            filePath = fileSettings.FilePath;

            _readerService = new ExcelReaderService();
            _writerService = new ExcelWriterService();
            _readerService = new ExcelReaderService();
            _writerService = new ExcelWriterService();

            List<ProductInfo> listProducts = _readerService.ReadExcelFile<ProductInfo>(fileLocation);

            List<ProductInfo> productsToTier = listProducts
                .GroupBy(p => p.Product)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            List<ProductInfo> productsWithTiers = listProducts
                .GroupBy(p => p.Product)
                .Where(g => g.Count() > 1) 
                .SelectMany(g => g)
                .ToList();

            foreach (var product in productsToTier)
            {
                if (product.QuantityPriceBreakTo == 0)
                {
                    product.QuantityPriceBreakTo = product.FillQuantity * 8;
                }
                productsWithTiers.Add(product);
                string jsonString = JsonSerializer.Serialize(product, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonString);

                ProductInfo newProduct = new ProductInfo()
                {
                    Product = product.Product,
                    PriceListName = product.PriceListName,
                    MtoPricingTiers = product.MtoPricingTiers,
                    UnitOfMeasure = product.UnitOfMeasure,
                    Price = ThirtyPercentDiscount(product.Price),
                    StartDate = product.StartDate,
                    FillQuantity = product.FillQuantity,
                    Status = product.Status,
                    MinLbs = product.MinLbs,
                    QuantityPriceBreakFrom = (product.FillQuantity * 8) + 1,
                    QuantityPriceBreakTo = 999999

                };
                jsonString = JsonSerializer.Serialize(newProduct, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("New Product JSON:");
                Console.WriteLine(jsonString);
                productsWithTiers.Add(newProduct);
            }

            _writerService.WriteExcelFile(filePath, productsWithTiers);
        }

        private static decimal ThirtyPercentDiscount(decimal price)
        {
            if (price <= 0)
            {
                throw new ArgumentException("Price must be greater than 0 to apply a discount.");
            }
            return Math.Round(price * 0.7m, 2);
        }
    }

    public class FileSettings
    {
        public string FileLocation { get; set; }
        public string FilePath { get; set; }
    }

    public class ProductInfo
    {
        [ExcelReaderService.ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelReaderService.ExcelColumn("Price List Name")]
        public string PriceListName { get; set; }

        [ExcelReaderService.ExcelColumn("Move to MTO Pricing and add TIERS")]
        public string MtoPricingTiers { get; set; }

        [ExcelReaderService.ExcelColumn("Product Description")]
        public string ProductDescription { get; set; }

        [ExcelReaderService.ExcelColumn("UOM")]
        public string UnitOfMeasure { get; set; }

        [ExcelReaderService.ExcelColumn("Price")]
        public decimal Price { get; set; }

        [ExcelReaderService.ExcelColumn("Start Date")]
        public DateTime? StartDate { get; set; }

        [ExcelReaderService.ExcelColumn("Fill Quantity")]
        public int FillQuantity { get; set; }

        [ExcelReaderService.ExcelColumn("Active /Inactive")]
        public string Status { get; set; }

        [ExcelReaderService.ExcelColumn("MIN LBS")]
        public int MinLbs { get; set; }

        [ExcelReaderService.ExcelColumn("Quantity Price Break From")]
        public int QuantityPriceBreakFrom { get; set; }

        [ExcelReaderService.ExcelColumn("Quantity Price Break To")]
        public int QuantityPriceBreakTo { get; set; }
    }

}
