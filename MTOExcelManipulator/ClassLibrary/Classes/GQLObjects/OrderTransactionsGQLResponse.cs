namespace ClassLibrary.Classes.GQLObjects
{
    public class OrderTransactionGQLResponse
    {
        public OrderData Order { get; set; }

        public class OrderData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string CreatedAt { get; set; }
            public List<Transaction> Transactions { get; set; }
        }

        public class Transaction
        {
            public string Id { get; set; }
            public string Amount { get; set; }
            public string Gateway { get; set; }
            public string Status { get; set; }
            public string CreatedAt { get; set; }
            public string Kind { get; set; }
            public string ProcessedAt { get; set; }
            public PaymentDetails PaymentDetails { get; set; }
        }

        public class PaymentDetails
        {
            public string CreditCardCompany { get; set; }
        }
    }
}
