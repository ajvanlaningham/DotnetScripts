using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes.GQLObjects
{
    public class OrdersByCustomerResponse
    {
        public CustomerNode Customer { get; set; }

        public class CustomerNode
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
            public List<LineItem> LineItems { get; set; }
        }

        public class LineItem
        {
            public string Title { get; set; }
            public Product Product { get; set; }
        }

        public class Product
        {
            public List<string> Tags { get; set; }
        }
    }
}
