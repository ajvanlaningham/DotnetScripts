namespace ClassLibrary.Classes.GQLObjects
{
    public class OrderTransactionGQLResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public TransactionOrder Order { get; set; }
    }

    public class TransactionOrder
    {
        public Transactions OrderTransactions { get; set; }
    }

    public class Transactions
    {
        public List<Edge> OrderTransactionEdges { get; set; }
    }

    public class Edge
    {
        public TransactionNode OrderTransactionsEdgesNode { get; set; }
    }

    public class TransactionNode
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Status { get; set; }
        public AmountSet AmountSet { get; set; }
    }

    public class AmountSet
    {
        public Money ShopMoney { get; set; }
        public Money PresentmentMoney { get; set; }
    }

    public class Money
    {
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
    }
}
