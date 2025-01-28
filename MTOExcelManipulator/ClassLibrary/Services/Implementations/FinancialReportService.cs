using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;

namespace ClassLibrary.Services.Implementations
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly IGraphQLClient _graphQLClient;
        public FinancialReportService(ShopifyAdminAPISettings settings) 
        {
            var endpoint = new Uri($"{EnsureProtocol(settings.StoreUrl)}{Constants.Constants.GraphQLAPIString}");

            var options = new GraphQLHttpClientOptions
            {
                EndPoint = endpoint
            };

            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", settings.AccessToken);

            _graphQLClient = graphQLClient;
        }

        public async Task<List<OrderTransaction>> RunFinancialConsolidationReport()
        {
            var gqlQuery = @"
query GetOrderTransactions {
  orders(first: 10) {
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

            var request = new GraphQLRequest
            {
                Query = gqlQuery,
            };

            var response = await _graphQLClient.SendQueryAsync<FinancialConsolidationReportResponse>(request);

            if (response?.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
            }

            var orderEdges = response?.Data?.Data?.Orders?.Edges;
            if (orderEdges == null || !orderEdges.Any())
            {
                throw new Exception("No data returned for the specified date range.");
            }

            var allPayouts = new List<OrderTransaction>();
            foreach (var edge in orderEdges)
            {
                allPayouts.AddRange(edge.Node.Transactions);
            }

            return allPayouts;
        }


        private static string EnsureProtocol(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                return $"https://{url}";
            }
            return url;
        }
    }
}
