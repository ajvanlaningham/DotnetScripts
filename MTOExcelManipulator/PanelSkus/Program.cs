using ClassLibrary.Services.Implementations;
using ShopifySharp;

namespace PanelSkus
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
            string filePath = config.GetValue("FilePaths:PanelSkuReport");

            List<Product> products = await  _prodService.FetchAllProductsAsync();

            products = products.Where(
                p => p.Variants.ToList().Any() &&
                p.Variants.FirstOrDefault().SKU != null &&
                p.Variants.FirstOrDefault().SKU.Contains("panel", StringComparison.OrdinalIgnoreCase) | 
                p.Variants.FirstOrDefault().SKU.Contains("SB", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine(products.Count);

            List<PanelReportObject> panels = new List<PanelReportObject>();

            foreach (var product in products)
            {
                PanelReportObject panel = new PanelReportObject()
                {
                    SKU = product.Variants.FirstOrDefault().SKU.Split('-').First(),
                    Title = product.Title,
                    ShopifyIDNumber =  product.Id.Value.ToString(),
                    Handle = product.Handle,
                    URL = $"https://powdercoatings.ppg.com/products/{product.Handle}"
                };
                panels.Add(panel);
            }


            _excelWriterService.WriteExcelFile(filePath, panels);
        }
    }

    public class PanelReportObject
    {
        public string SKU { get; set; }
        public string Title { get; set; }
        public string Handle { get; set; }
        public string URL { get; set; }
        public string ShopifyIDNumber { get; set; }
    }
}
