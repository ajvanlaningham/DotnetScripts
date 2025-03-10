namespace RefinishCustomers
{
    public class RefinishCustomer
    {
        public string PriceListName { get; set; }
        public string CustomerAccountNumber { get; set; }
        public RefinishCustomerType CustomerType { get; set; }
        public RefinishPriceList PriceList { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is RefinishCustomer other)
            {
                return CustomerAccountNumber == other.CustomerAccountNumber &&
                       PriceListName == other.PriceListName &&
                       CustomerType == other.CustomerType &&
                       PriceList == other.PriceList;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CustomerAccountNumber, PriceListName, CustomerType, PriceList);
        }
    }


    public enum RefinishCustomerType
    {
        Distributor,
        InsideSales,
        Other
    }

    public enum RefinishPriceList
    {
        DistributorList,
        InsideSalesList,
        Other
    }

}
