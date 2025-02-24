using ClassLibrary.Services.Implementations;
using System.ComponentModel.DataAnnotations;
using static ClassLibrary.Services.Implementations.ExcelReaderService;

namespace BXCPrices
{
    internal class Program
    {
        private static ExcelReaderService _excelReaderService;
        private static ExcelWriterService _excelWriterService;
        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelReaderService = new ExcelReaderService();
            _excelWriterService = new ExcelWriterService();
            string PricingMessageFilePath = config.GetValue("FilePaths:PricingProducts");
            string BXCMessageFilePath = config.GetValue("FilePaths:BXCProducts");
            string BXCFinalMessageFilePath = config.GetValue("FilePaths:ReportPath");


            List<PricingMessage> pricingMessages = _excelReaderService.ReadExcelFile<PricingMessage>(PricingMessageFilePath);
            pricingMessages = pricingMessages.Where(p => p.Item.Contains("BXC")).ToList();

            List<BXCPricingObj> bXCPricingObjs = _excelReaderService.ReadExcelFile<BXCPricingObj>(BXCMessageFilePath);
            List<BXCPricingObj> manipulatedObjs = new List<BXCPricingObj>();
            List<BXCPricingObj> updateList = new List<BXCPricingObj>();
            List<string> missed = new List<string>();

            foreach (var item in bXCPricingObjs)
            {
                BXCPricingObj newObj = item;
                newObj.ItemNumber = $"{item.ItemNumber}/BXC";
                newObj.SmallBatchPrice = item.SmallBatchPrice;
                newObj.TwentyFiveLbPrice = item.TwentyFiveLbPrice;
                manipulatedObjs.Add(newObj);
            }

            foreach (BXCPricingObj bxcObj in manipulatedObjs)
            {
                PricingMessage message = pricingMessages.Find(m => m.Item.Contains(bxcObj.ItemNumber));

                if (message is not null)
                {
                    bxcObj.EcomOraclePriceListCheck = message.UnitPrice;
                    bxcObj.SmallBatchPrice = message.UnitPrice;
                    
                }
                else
                {
                    missed.Add(bxcObj.ItemNumber);
                    bxcObj.EcomOraclePriceListCheck = "not found";
                }
                updateList.Add(bxcObj);
            }

            _excelWriterService.WriteExcelFile(BXCFinalMessageFilePath, updateList);
                    
        }
    }

    public class PricingMessage
    {
        [ExcelColumn("name")]
        public string Name { get; set; }

        [ExcelColumn("list_header_id")]
        public string ListHeaderId { get; set; }

        [ExcelColumn("item")]
        public string Item { get; set; }

        [ExcelColumn("uprice")]
        public string UnitPrice { get; set; }

        [ExcelColumn("uom")]
        public string UnitOfMeasure { get; set; }

        [ExcelColumn("start_date_active")]
        public string StartDateActive { get; set; }
    }

    public class BXCPricingObj
    {
        [ExcelColumn("ItemNumber")]
        public string ItemNumber { get; set; }

        [ExcelColumn("ItemName")]
        public string ItemName { get; set; }

        [ExcelColumn("SKU")]
        public string SKU { get; set; }

        [ExcelColumn("Small Batch Price")]
        public string SmallBatchPrice { get; set; }

        [ExcelColumn("25 Lb Price")]
        public string TwentyFiveLbPrice { get; set; }

        [ExcelColumn("Ecom Oracle Price List Check")]
        public string EcomOraclePriceListCheck { get; set; }
    }
}
