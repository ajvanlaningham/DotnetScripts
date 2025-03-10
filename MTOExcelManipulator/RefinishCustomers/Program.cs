using ClassLibrary.Classes.OracleMessages;
using ClassLibrary.Helpers;
using ClassLibrary.Services.Implementations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RefinishCustomers
{
    internal class Program
    {
        private static ExcelWriterService _excelWriterService;
        private static CustomCustomerService _customerService;

        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelWriterService = new ExcelWriterService();
            _customerService = new CustomCustomerService(config.GetShopifySettings());
            string directory = config.GetValue("FilePaths:AdjustmentFiles");

            List<string> jsonFiles = FileHelper.GetAllJsonFiles(directory);
            HashSet<Adjustment> adjustments = new HashSet<Adjustment>();
            foreach (var file in jsonFiles)
            {
                try
                {
                    string jsonContent = await File.ReadAllTextAsync(file);
                    var rootObject = JsonSerializer.Deserialize<Root>(jsonContent);

                    if (rootObject?.AdjustmentList?.Adjustments != null)
                    {
                        foreach (var adjustment in rootObject.AdjustmentList.Adjustments)
                        {
                            adjustments.Add(adjustment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }

            List<Adjustment> adjustmentList = adjustments
                .Where(a => a.PriceListName == "REF - US IS CUSTOMER STANDARD PRICE LIST"
                         || a.PriceListName == "REF US PPG DISTRIBUTOR")
                .ToList();

            HashSet<RefinishCustomer> InsideSalesCustomers = new HashSet<RefinishCustomer>();
            HashSet<RefinishCustomer> DistributorCustomers = new HashSet<RefinishCustomer>();
            HashSet<RefinishCustomer> OtherCustomers = new HashSet<RefinishCustomer>();
            foreach ( var adjustment in adjustmentList)
            {
                RefinishCustomer customer = new RefinishCustomer()
                {
                    CustomerAccountNumber = adjustment.CustAccountNumber,
                    PriceListName = adjustment.PriceListName,
                };

                switch (customer.PriceListName)
                {
                    case "REF US PPG DISTRIBUTOR":
                        customer.PriceList = RefinishPriceList.DistributorList;
                        break;
                    case "REF - US IS CUSTOMER STANDARD PRICE LIST":
                        customer.PriceList = RefinishPriceList.InsideSalesList;
                        break;
                    default:
                        customer.PriceList = RefinishPriceList.Other;
                        break;
                }

                if (customer.CustomerAccountNumber.Contains("REF"))
                {
                    customer.CustomerType = RefinishCustomerType.Distributor;
                }
                else if (customer.CustomerAccountNumber.Contains("INC"))
                {
                    customer.CustomerType = RefinishCustomerType.InsideSales;
                }
                else
                {
                    customer.CustomerType = RefinishCustomerType.Other;
                }

                switch (customer.CustomerType)
                {
                    case RefinishCustomerType.Distributor:
                        DistributorCustomers.Add(customer);
                        break;
                    case RefinishCustomerType.InsideSales:
                        InsideSalesCustomers.Add(customer);
                        break;
                    case RefinishCustomerType.Other:
                        OtherCustomers.Add(customer);
                        break;
                }
            }

            foreach ( var distributor in DistributorCustomers )
            {
                Console.WriteLine($"Distributor Customer: {distributor.CustomerAccountNumber}");
            }

            foreach (var insideSales in InsideSalesCustomers)
            {
                Console.WriteLine($"InsideSales Customer: {insideSales.CustomerAccountNumber}");
            }

            foreach (var other in OtherCustomers)
            {
                Console.WriteLine($"Other Customer: {other.CustomerAccountNumber}");
            }

        }
    }

}
