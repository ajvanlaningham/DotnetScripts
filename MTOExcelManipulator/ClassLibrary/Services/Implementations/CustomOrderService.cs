using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using ShopifySharp;
using static ClassLibrary.Classes.GQLObjects.OrderByIDResponse;
using static OrdersByCustomerResponse;

namespace ClassLibrary.Services.Implementations
{
    public class CustomOrderService : ICustomOrderService
    {
        private readonly IGraphQLClient _graphQLClient;
        private readonly ShopifySharp.OrderService _service;
        private readonly ShopifySharp.ShopifyPaymentsService _paymentsService;
        private readonly RateLimiter _limiter;

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
            _paymentsService = new ShopifySharp.ShopifyPaymentsService(settings.StoreUrl, settings.AccessToken);
            _limiter = new RateLimiter(4, TimeSpan.FromSeconds(10));
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

        public async Task<List<ShopifyPaymentsPayout>> FetchAllPayoutsAsync()
        {
            var payoutsList = new List<ShopifyPaymentsPayout>();
            string? startingAfter = null;

            while (true)
            {
                await _limiter.PerformAsync(async () =>
                {
      
                    var tempPayoutsList = await _paymentsService.ListPayoutsAsync();

                    if (tempPayoutsList != null && tempPayoutsList.Items.Any())
                    {
                        payoutsList.AddRange(tempPayoutsList.Items);
                        startingAfter = tempPayoutsList.Items.Last().Id.ToString();
                    }
                    else
                    {
                        startingAfter = null;
                    }
                });

                if (string.IsNullOrEmpty(startingAfter))
                    break;
            }

            return payoutsList;
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

        public async Task<string> FetchOrderJsonStringByIDAsync(long orderID) =>
            Newtonsoft.Json.JsonConvert.SerializeObject(await _service.GetAsync(orderID));

        public async Task<ShopifySharp.Order> FetchOrderAsync(long orderID) =>
            await _service.GetAsync(orderID);

        public async Task<OrderNode> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            var query = new GraphQLHttpRequest
            {
                Query = @"
        query getOrderById($id: ID!) {
          order(id: $id) {
            id
            name
            poNumber
            note
            createdAt
            tags
            billingAddress {
              company
            }
            purchasingEntity {
              __typename
              ... on PurchasingCompany {
                location {
                  id
                  metafield(namespace: ""custom"", key: ""shiptoid"") {
                    value
                  }
                }
              }
              ... on Customer {
                id
                firstName
                lastName
                email
                phone
                tags
              }
            }
            shippingAddress {
              firstName
              company
              lastName
              province
              countryCodeV2
              address1
              address2
              city
              country
              zip
            }
            shippingLines(first: 10) {
              edges {
                node {
                  title
                  price
                  code
                }
              }
            }
            lineItems(first: 250) {
              edges {
                node {
                  id
                  fulfillableQuantity
                  fulfillmentStatus
                  name
                  originalUnitPriceSet {
                    shopMoney {
                      amount
                      currencyCode
                    }
                    presentmentMoney {
                      amount
                      currencyCode
                    }
                  }
                  product {
                    id
                  }
                  quantity
                  requiresShipping
                  sku
                  taxable
                  title
                  totalDiscount
                  totalDiscountSet {
                    shopMoney {
                      amount
                      currencyCode
                    }
                    presentmentMoney {
                      amount
                      currencyCode
                    }
                  }
                  variantTitle
                  vendor
                  taxLines {
                    title
                    price
                    rate
                  }
                  discountAllocations {
                    allocatedAmount {
                      amount
                      currencyCode
                    }
                  }
                }
              }
            }
            paymentGatewayNames
            fulfillmentOrders(first: 10) {
                  edges {
                    node {
                      id
                      assignedLocation {
                        location {
                          id
                          name
                        }
                      }
                    }
                  }
                }
            }
          }",
                Variables = new { id = orderId }
            };

            var response = await _graphQLClient.SendQueryAsync<OrderByIdQueryResponse>(query, cancellationToken);
            return response.Data.Order;
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
