using ClassLibrary.Classes;
using ClassLibrary.Services.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using ShopifySharp;
using ShopifySharp.Filters;
using System;


namespace ClassLibrary.Services.Implementations
{
    public class CustomCustomerService : ICustomCustomerService
    {
        private readonly CustomerService _service;
        private readonly RateLimiter _limiter;
        private readonly IGraphQLClient _graphQLClient;

        public CustomCustomerService(CustomerService service, RateLimiter limiter, IGraphQLClient graphQLClient)
        {
            _service = service;
            _limiter = limiter;
        }

        public CustomCustomerService(ShopifyAdminAPISettings settings)
        {
            _service = new CustomerService(settings.StoreUrl, settings.AccessToken);
            _limiter = new RateLimiter(4, TimeSpan.FromSeconds(10));

            var endpoint = new Uri($"{EnsureProtocol(settings.StoreUrl)}/admin/api/2025-01/graphql.json");

            var options = new GraphQLHttpClientOptions
            {
                EndPoint = endpoint
            };

            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", settings.AccessToken);

            _graphQLClient = graphQLClient;
        }

        public async Task UpdateCustomerTagsAsync(ShopifySharp.Customer cust, string tag)
        {
            await _limiter.PerformAsync(async () =>
            {
                await _service.UpdateAsync(cust.Id.Value, new ShopifySharp.Customer()
                {
                    Tags = $"{cust.Tags},{tag}",
                });
                Console.WriteLine($"Updated customer tags for {cust.Email}");
            });
        }

        public async Task<List<ShopifySharp.Customer>> FetchAllCustomersAsync()
        {
            var customerList = new List<ShopifySharp.Customer>();
            long? lastId = 0;

            while (lastId >= 0)
            {
                await _limiter.PerformAsync(async () =>
                {
                    var filter = new CustomerListFilter
                    {
                        SinceId = lastId,
                        Limit = 250
                    };

                    var tempProductList = await _service.ListAsync(filter);
                    if (tempProductList != null && tempProductList.Items.Any())
                    {
                        customerList.AddRange(tempProductList.Items);
                        lastId = tempProductList.Items.Last().Id;
                    }
                    else
                    {
                        lastId = null;
                    }
                });
            }

            return customerList;
        }

        public async Task<Classes.GQLObjects.Customer> GetCustomerByIdAsync(string customerId)
        {
            long.TryParse(customerId, out long numericId);

            string gid = $"gid://shopify/Customer/{numericId}";
            var query = new GraphQLHttpRequest
            {
                Query = @"
            query getCustomerById($customerId: ID!) {
              customer(id: $customerId) {
                id
                email
                firstName
                lastName
                multipassIdentifier
                lastOrder {
                  id
                }
                note
                phone
                state
                tags
                metafields(first: 10) {
                  edges {
                    node {
                      id
                      key
                      value
                      type
                      namespace
                      description
                    }
                  }
                }
              }
            }",
                Variables = new { customerId = gid }
            };

            var response = await _graphQLClient.SendQueryAsync<Classes.GQLObjects.GetCustomerByIDQueryResponse>(query);
            return response.Data.Customer;
        }

        public async Task<bool> UpdateCustomerMetafieldAsync(string customerId, string metafieldNamespace, string metafieldKey, string metafieldType, string newValue)
        {
            var mutation = new GraphQLHttpRequest
            {
                Query = @"
            mutation updateCustomerMetafield($input: CustomerInput!) {
              customerUpdate(input: $input) {
                customer {
                  id
                  metafields(first: 10) {
                    edges {
                      node {
                        namespace
                        key
                        value
                        type
                      }
                    }
                  }
                }
                userErrors {
                  field
                  message
                }
              }
            }",
                Variables = new
                {
                    input = new
                    {
                        id = customerId,
                        metafields = new List<object>
                        {
                            new
                            {
                                @namespace = metafieldNamespace,
                                key = metafieldKey,
                                value = newValue,
                                type = metafieldType
                            }
                        }
                    }
                }
            };

            var response = await _graphQLClient.SendMutationAsync<CustomerUpdateResponse>(mutation);

            return response.Data?.CustomerUpdate?.UserErrors?.Count == 0;
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
