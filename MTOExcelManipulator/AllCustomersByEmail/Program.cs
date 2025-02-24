using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Implementations;
using ShopifySharp;

namespace AllCustomersByEmail
{
    internal class Program
    {

        private static ExcelWriterService _excelWriterService;
        private static CustomCustomerService _customCustomerService;

        static async Task Main(string[] args)
        {
            ConfigurationService config = new ConfigurationService("appsettings.json");
            _excelWriterService = new ExcelWriterService();

            string filePath = config.GetValue("FilePaths:SalesforceCustomerExcelLocation");


            _customCustomerService = new CustomCustomerService(config.GetShopifySettings());

            //List<CustomerFetch> customers = await _customCustomerService.FetchAllCustomersAsync();
            //List<SalesforceCustomer> salesforceCustomers = new List<SalesforceCustomer>();

            //foreach (CustomerFetch customer in customers)
            //{
            //    //SalesforceCustomer sCustomer = MapCustomerToSalesforceCustomer(customer);
            //    //salesforceCustomers.Add(sCustomer);
            //}
            
            //_excelWriterService.WriteExcelFile("", salesforceCustomers);
        }

        //private static SalesforceCustomer MapCustomerToSalesforceCustomer(CustomerFetch customer)
        //{
        //    return new SalesforceCustomer()
        //    {
        //        ShopifyCustomerId = customer.Id,
        //        FirstName = customer.FirstName?? "",
        //        LastName = customer.LastName?? "",
        //        Email = customer.Email?? "",
        //        Phone = customer.Phone?? "",
        //        AddressLine1 = customer.Addresses.FirstOrDefault().Address1?? "",
        //        AddressLine2 = customer.Addresses.FirstOrDefault().Address2?? "",
        //        City = customer.Addresses.FirstOrDefault().City?? "",
        //        StateOrProvince = customer.Addresses.FirstOrDefault().Province?? "",
        //        PostalCode = customer.Addresses.FirstOrDefault().ProvinceCode ?? "",

        //        Tags = customer.Tags?? "",

        //        Company = customer.Addresses.FirstOrDefault().Company ?? "",
        //        Notes = customer.Note
        //    };
        //}
    }

    public class SalesforceCustomer
    {
        // Unique identifiers
        public string ShopifyCustomerId { get; set; }
        public string SalesforceCustomerId { get; set; }

        // Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Address Information
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Customer Metadata
        public string Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Additional fields
        public string Company { get; set; }
        public string Notes { get; set; }

        // Integration fields
        public bool IsSyncedToSalesforce { get; set; }
        public DateTime LastSyncedAt { get; set; }
    }

}
