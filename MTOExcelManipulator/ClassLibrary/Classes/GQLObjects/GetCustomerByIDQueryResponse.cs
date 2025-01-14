using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes.GQLObjects
{
    public class GetCustomerByIDQueryResponse
    {
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MultipassIdentifier { get; set; }
        public long? LastOrderId { get; set; }
        public string Note { get; set; }
        public string Phone { get; set; }
        public string State { get; set; }
        public string[] Tags { get; set; }
        public decimal? TotalSpent { get; set; }
        public MetaFieldConnection Metafields { get; set; }
    }

    public class MetaFieldConnection
    {
        public List<MetaFieldEdge> Edges { get; set; }
    }

    public class MetaFieldEdge
    {
        public MetaField Node { get; set; }
    }

    public class MetaField
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string Namespace { get; set; }
        public string Description { get; set; }
    }
}
