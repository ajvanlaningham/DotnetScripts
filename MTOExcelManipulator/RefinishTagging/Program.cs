using ClassLibrary.Services.Implementations;
using ShopifySharp.GraphQL;

namespace RefinishTagging
{
    internal class Program
    {

        private static ExcelWriterService _excelWriterService;
        private static CustomCustomerService _custService;
        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelWriterService = new ExcelWriterService();
            _custService = new CustomCustomerService(config.GetShopifySettings());
            string filePath = config.GetValue("FilePaths:RefinishCustoemrsReport");

            List<ClassLibrary.Classes.GQLObjects.Customer> customers = await _custService.FetchAllCustomersAsync();

            customers = customers.Where(c => c.Tags.Any(tag => tag.Contains("REF", StringComparison.OrdinalIgnoreCase))).ToList();

            Console.WriteLine(customers.Count);
            List<RefinishCustomerObject> refinishCustomers = new List<RefinishCustomerObject>();
            foreach(var customer in customers)
            {
                var suids = string.Join(",", customer.Tags.Where(tag => tag.Contains("SUID", StringComparison.OrdinalIgnoreCase)));
                var arNumber = customer.Tags.FirstOrDefault(tag => tag.Contains("REF", StringComparison.OrdinalIgnoreCase));

                RefinishCustomerObject newRefinishCustomer = new RefinishCustomerObject()
                {
                    ID = customer.Id,
                    Email = customer.Email,
                    ARNumber =arNumber,
                    Address = customer.State,
                    Tags = string.Join(",", customer.Tags),
                    SUIDs = suids,
                };
                refinishCustomers.Add(newRefinishCustomer);
            }

            //_excelWriterService.WriteExcelFile(filePath, refinishCustomers);

            foreach (var cust in refinishCustomers)
            {
                await _custService.UpdateCustomerTagsUsingGQLAsync(cust.ID, "REF-DIST");
            }
            
        }
    }

    public class RefinishCustomerObject
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string ARNumber { get; set; }
        public string Address { get; set; }
        public string Tags { get; set; }
        public string SUIDs { get; set; }
    }
}
