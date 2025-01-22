public class OrdersByCustomerResponse
{
    public CustomerResponse Customer { get; set; }

    public class CustomerResponse
    {
        public OrdersConnection Orders { get; set; }
    }

    public class OrdersConnection
    {
        public List<OrderEdge> Edges { get; set; }
    }

    public class OrderEdge
    {
        public OrderDetail Node { get; set; }
    }

    public class OrderDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientIp { get; set; }
        public LineItemsConnection LineItems { get; set; }
    }

    public class LineItemsConnection
    {
        public List<LineItemEdge> Edges { get; set; }
    }

    public class LineItemEdge
    {
        public LineItemNode Node { get; set; }
    }

    public class LineItemNode
    {
        public string Title { get; set; }
        public ProductDetail Product { get; set; }
    }

    public class ProductDetail
    {
        public string Tags { get; set; }  // Shopify may return tags as a single string of comma-separated values.
    }
}
