using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using static OrdersByCustomerResponse;

namespace ClassLibrary.Services.Implementations
{
    public class CustomOrderService : ICustomOrderService
    {
        private readonly IGraphQLClient _graphQLClient;

        public CustomOrderService(ShopifyAdminAPISettings settings)
        {
            var endpoint = new Uri($"{EnsureProtocol(settings.StoreUrl)}/admin/api/2025-01/graphql.json");

            var options = new GraphQLHttpClientOptions
            {
                EndPoint = endpoint
            };

            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", settings.AccessToken);

            _graphQLClient = graphQLClient;
        }

        public async Task<List<OrderDetail>> GetOrdersByCustomerIdAsync(string customerGid)
        {
            var query = new GraphQLHttpRequest
            {
                Query = @"
            query getOrders($customerId: ID!) {
              customer(id: $customerId) {
                orders(first: 100) {
                  edges {
                    node {
                      id
                      name
                      clientIp
                      lineItems(first: 100) {
                        edges {
                          node {
                            title
                            product {
                              tags
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }",
                Variables = new { customerId = customerGid }
            };

            var response = await _graphQLClient.SendQueryAsync<OrdersByCustomerResponse>(query);
            return response.Data?.Customer?.Orders?.Edges.Select(e => e.Node).ToList() ?? new List<OrderDetail>();
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
