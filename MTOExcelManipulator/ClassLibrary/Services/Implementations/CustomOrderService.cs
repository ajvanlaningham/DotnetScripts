using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using System.Text;
using static OrdersByCustomerResponse;

namespace ClassLibrary.Services.Implementations
{
    public class CustomOrderService : ICustomOrderService
    {
        private readonly IGraphQLClient _graphQLClient;
        private readonly ShopifySharp.OrderService _service;

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
            _service = new ShopifySharp.OrderService(settings.StoreUrl, settings.AccessToken);
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

        public async Task<OrderTransactionGQLResponse.OrderData> GetOrderTransactionsAsync(string orderId)
        {
            var query = new GraphQLHttpRequest
            {
                Query = @"
        query GetOrderTransactions($orderId: ID!) {
            order(id: $orderId) {
                id
                name
                createdAt
                transactions {
                    id
                    amount
                    gateway
                    status
                    createdAt
                    kind
                    processedAt

                }
            }
        }",
                Variables = new { orderId = $"gid://shopify/Order/{orderId}" }
            };

            var response = await _graphQLClient.SendQueryAsync<OrderTransactionGQLResponse>(query);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
            }

            return response.Data?.Order;
        }

        public async Task<PayoutTransactionGQLResponse> GetPayoutTransactionAsync(string payoutId)
        {
            var query = new GraphQLHttpRequest
            {
                Query = @"
        query GetShopifyPaymentsPayout($id: ID!) {
            order(id: $orderId) {
... on ShopifyPaymentsPayout {
                id
                name
                createdAt
                transactions {
                    id
                    amount
                    gateway
                    status
                    createdAt
                    kind
                    processedAt

                }
            }
        }",
                Variables = new { payoutId = $"gid://shopify/Order/{payoutId}" }
            };

            throw new NotImplementedException();

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
