﻿using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using ShopifySharp;
using ShopifySharp.Filters;
using System;
using System.Text.Json;


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

        public async Task UpdateCustomerTagsUsingGQLAsync(string customerGQLId, string tag)
        {
            var customer = await GetCustomerByIdAsync(customerGQLId);

            List<string> updatedTags = customer.Tags != null
                ? new List<string>(customer.Tags)
                : new List<string>();

            if (!updatedTags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                updatedTags.Add(tag);
            }

            var mutation = new GraphQLHttpRequest
            {
                Query = @"
            mutation updateCustomerTags($input: CustomerInput!) {
                customerUpdate(input: $input) {
                    customer {
                        id
                        tags
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
                        id = customerGQLId,
                        tags = updatedTags
                    }
                }
            };

            var response = await _graphQLClient.SendMutationAsync<CustomerUpdateResponse>(mutation);

            if (response.Data.CustomerUpdate.UserErrors.Count > 0)
            {
                Console.WriteLine($"Error updating customer tags: {string.Join(", ", response.Data.CustomerUpdate.UserErrors.Select(e => e.Message))}");
            }
            else
            {
                Console.WriteLine($"Updated customer tags for {customer.Email}");
            }
        }


        public async Task<List<Classes.GQLObjects.Customer>> FetchAllCustomersAsync()
        {
            var allCustomers = new List<Classes.GQLObjects.Customer>();
            string? cursor = null;
            bool hasMore = true;

            while (hasMore)
            {
                var query = new GraphQLHttpRequest
                {
                    Query = @"
                query($cursor: String) {
                  customers(first: 250, after: $cursor) {
                    edges {
                      cursor
                      node {
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
                              namespace
                              description
                            }
                          }
                        }
                      }
                    }
                    pageInfo {
                      hasNextPage
                    }
                  }
                }",
                    Variables = new { cursor }
                };

                var response = await _graphQLClient.SendQueryAsync<CustomerResponseData>(query);
                var customers = response.Data.Customers.Edges.Select(edge => edge.Node).ToList();

                allCustomers.AddRange(customers);
                hasMore = response.Data.Customers.PageInfo.HasNextPage;

                if (hasMore)
                {
                    cursor = response.Data.Customers.Edges.Last().Cursor;
                }
            }

            return allCustomers;
        }
    

        public async Task<Classes.GQLObjects.Customer> GetCustomerByIdAsync(string customerId)
        {
            string gid = customerId.StartsWith("gid://shopify/Customer/")
                ? customerId
                : $"gid://shopify/Customer/{customerId}";

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
