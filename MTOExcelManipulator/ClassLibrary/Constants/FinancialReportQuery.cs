namespace ClassLibrary.Constants
{
    public static class FinancialReportQuery
    {
        public const string Query = @"
query GetOrderTransactions($startDate: DateTime, $endDate: DateTime) {
  orders(first: 100, query: ""processed_at:>=$startDate processed_at:<=$endDate"") {
    edges {
      node {
        id
        name
        transactions {
          id
          processedAt
          amount
          gateway
          status
          kind
        }
      }
    }
  }
}";
    }
}
