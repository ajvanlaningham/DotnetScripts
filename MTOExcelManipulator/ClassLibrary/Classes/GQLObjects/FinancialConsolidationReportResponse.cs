using System.Text.Json.Serialization;

namespace ClassLibrary.Classes.GQLObjects
{

    public class FinancialConsolidationReportResponse
    {
        [JsonPropertyName("data")]
        public FinancialReportData Data { get; set; }
    }

    public class FinancialReportData
    {
        public OrdersConnection Orders { get; set; } // The top-level orders field
    }

    public class OrdersConnection
    {
        public List<OrderEdge> Edges { get; set; }   // List of orders
    }

    public class OrderEdge
    {
        public Order Node { get; set; }              // The order data
    }

    public class Order
    {
        public string Id { get; set; }               // Order ID
        public string Name { get; set; }             // Order number (e.g., "#1001")
        public List<OrderTransaction> Transactions { get; set; } // List of transactions
    }

    public class OrderTransaction
    {
        public string Id { get; set; }                // Transaction ID
        public DateTime ProcessedAt { get; set; }    // Transaction date
        public MoneyV2 Amount { get; set; }          // Transaction amount
        public string Gateway { get; set; }         // Payment method (gateway)
        public string Status { get; set; }          // Status of the transaction
        public string Kind { get; set; }            // Type of transaction (sale, refund, etc.)
    }

    public class MoneyV2
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public class PageInfo
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }
    }
}