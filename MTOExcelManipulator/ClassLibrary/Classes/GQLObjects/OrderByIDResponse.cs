using Newtonsoft.Json;
using ShopifySharp;
using ShopifySharp.GraphQL;
using System;
using System.Collections.Generic;

namespace ClassLibrary.Classes.GQLObjects
{
    public class OrderByIDResponse
    {
        public class OrderByIdQueryResponse
        {
            public OrderNode Order { get; set; }
        }

        public class OrderConnection
        {
            public List<OrderEdge> Edges { get; set; }
            public PageInfo PageInfo { get; set; }
        }

        public class OrderEdge
        {
            public string Cursor { get; set; }
            public OrderNode Node { get; set; }
        }

        public class OrderNode
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string PoNumber { get; set; }
            public string Note { get; set; }
            public DateTime CreatedAt { get; set; }
            public string[] Tags { get; set; }
            public Address ShippingAddress { get; set; }
            public OrderBillingAddress BillingAddress { get; set; }
            public ShippingLineConnection ShippingLines { get; set; }
            public LineItemConnection LineItems { get; set; }
            public string[] PaymentGatewayNames { get; set; }
            public List<FullfillmentOrderNode> Fulfillments { get; set; }

            // New: purchasingEntity union field.
            public PurchasingEntity PurchasingEntity { get; set; }
        }

        public class FullfillmentOrderEdge
        {
            public FullfillmentOrderNode Node { get; set; }
        }

        public class FullfillmentOrderNode
        {
            public string Id { get; set; }
            public AssignedLocation AssignedLocation { get; set; }
        }

        public class AssignedLocation
        {
            // This uses the ShopifySharp provided Location type.
            public ShopifySharp.GraphQL.Location Location { get; set; }
        }

        public class ShippingLineConnection
        {
            public List<ShippingLineEdge> Edges { get; set; }
        }

        public class ShippingLineEdge
        {
            public ShippingLine Node { get; set; }
        }

        public class ShippingLine
        {
            public string Title { get; set; }
            public decimal Price { get; set; }
            public string Code { get; set; }
        }

        public class OrderBillingAddress
        {
            public string Company { get; set; }
        }

        public class Address
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CountryCodeV2 { get; set; }
            public string Province { get; set; }
            public string Company { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Zip { get; set; }
        }

        public class LineItemConnection
        {
            public List<LineItemEdge> Edges { get; set; }
        }

        public class LineItemEdge
        {
            public LineItemNode Node { get; set; }
        }

        public class LineItemNode
        {
            public string Id { get; set; }
            public int FulfillableQuantity { get; set; }
            public string FulfillmentStatus { get; set; }
            public int Grams { get; set; }
            public string Name { get; set; }
            public PriceSet OriginalUnitPriceSet { get; set; }
            public LineItemProduct Product { get; set; }
            public int Quantity { get; set; }
            public bool RequiresShipping { get; set; }
            public string SKU { get; set; }
            public bool Taxable { get; set; }
            public string Title { get; set; }
            public string TotalDiscount { get; set; }
            public PriceSet TotalDiscountSet { get; set; }
            public string VariantTitle { get; set; }
            public string Vendor { get; set; }
            public List<TaxLine> TaxLines { get; set; }
            public List<DiscountAllocation> DiscountAllocations { get; set; }
        }

        public class LineItemProduct
        {
            public string Id { get; set; }
        }

        public class PriceSet
        {
            public Money ShopMoney { get; set; }
            public Money PresentmentMoney { get; set; }
        }

        public class Money
        {
            public string Amount { get; set; }
            public string CurrencyCode { get; set; }
        }

        public class TaxLine
        {
            public string Title { get; set; }
            public string Price { get; set; }
            public decimal Rate { get; set; }
        }

        public class DiscountAllocation
        {
            public Money AllocatedAmount { get; set; }
        }

        [JsonConverter(typeof(PurchasingEntityHelper))]
        public abstract class PurchasingEntity
        {
            public string __typename { get; set; }
        }

        public class PurchasingCompanyEntity : PurchasingEntity
        {
            public CompanyLocation Location { get; set; }
        }

        public class PurchasingCustomerEntity : PurchasingEntity
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string[] Tags { get; set; }
        }

        public class CompanyLocation
        {
            public string Id { get; set; }
            public Metafield Metafield { get; set; }
        }

        public class Metafield
        {
            public string Value { get; set; }
        }
    }
}
