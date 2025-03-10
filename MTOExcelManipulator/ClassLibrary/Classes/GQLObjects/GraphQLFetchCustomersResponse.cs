namespace ClassLibrary.Classes.GQLObjects
{
    public class CustomerResponseData
    {
        public CustomerConnection Customers { get; set; }
    }

    public class CustomerConnection
    {
        public List<CustomerConnectionEdge> Edges { get; set; }
        public PageInfo PageInfo { get; set; }
    }

    public class CustomerConnectionEdge
    {
        public Customer Node { get; set; }
        public string Cursor { get; set; }
    }
}
