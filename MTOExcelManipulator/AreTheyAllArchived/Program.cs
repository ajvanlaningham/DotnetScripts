using ClassLibrary.Services.Implementations;
using ShopifySharp;
using System.Drawing;

namespace AreTheyAllArchived
{
    internal class Program
    {
        private static ExcelReaderService _excelReaderService;
        private static CustomProductService _prodService;

        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelReaderService = new ExcelReaderService();
            _prodService = new CustomProductService(config.GetShopifySettings());
            string filePath = config.GetValue("FilePaths:BXCSkuReport");

            List<Product> products = await _prodService.FetchAllProductsAsync();

            List<BasePriceObj> bxcs = _excelReaderService.ReadExcelFile(filePath);


        }


    }

    public class BasePriceObj
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ListHeaderId { get; set; }
        public string Item { get; set; }
        public string Uprice { get; set; }
        public string Uom { get; set; }
        public string MinQty { get; set; }
        public string MaxQty { get; set; }
        public DateTime StartDateActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
