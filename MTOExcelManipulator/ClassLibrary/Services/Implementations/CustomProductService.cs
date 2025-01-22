using ClassLibrary.Classes;
using ClassLibrary.Classes.GQLObjects;
using ClassLibrary.Services.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using ShopifySharp;
using ShopifySharp.Filters;

namespace ClassLibrary.Services.Implementations
{
    public class CustomProductService : ICustomProductService
    {
        private readonly ProductService _service;
        private readonly RateLimiter _limiter;
        private readonly IGraphQLClient _client;

        public CustomProductService(ProductService service, RateLimiter limiter)
        {
            _service = service;
            _limiter = limiter;
        }

        public CustomProductService(ShopifyAdminAPISettings settings)
        {
            _service = new ProductService(settings.StoreUrl, settings.AccessToken);
            _limiter = new RateLimiter(4, TimeSpan.FromSeconds(10));

            var endpoint = new Uri($"{EnsureProtocol(settings.StoreUrl)}/admin/api/2025-01/graphql.json");

            var options = new GraphQLHttpClientOptions
            {
                EndPoint = endpoint
            };

            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", settings.AccessToken);

            _client = graphQLClient;
        }

        public List<string> TagList(ShopifySharp.Product product)
        {
            return string.IsNullOrWhiteSpace(product.Tags)
                ? new List<string>()
                : product.Tags.Split(',')
                              .Select(tag => tag.Trim())
                              .ToList();
        }

        public async Task<List<ShopifySharp.Product>> FetchAllProductsAsync()
        {
            var productList = new List<ShopifySharp.Product>();
            long? lastId = 0;

            while (lastId.HasValue)
            {
                await _limiter.PerformAsync(async () =>
                {
                    var filter = new ProductListFilter
                    {
                        SinceId = lastId,
                        Limit = 250
                    };

                    var tempProductList = await _service.ListAsync(filter);
                    if (tempProductList != null && tempProductList.Items.Any())
                    {
                        productList.AddRange(tempProductList.Items);
                        lastId = tempProductList.Items.Last().Id;
                    }
                    else
                    {
                        lastId = null;
                    }
                });
            }

            return productList;
        }

        public async Task ArchiveProductsAsync(IEnumerable<long> productIds)
        {
            int count = productIds.Count();
            foreach (var productId in productIds)
            {
                Console.WriteLine($"Archiving product {count--}");

                string mutation = @"
        mutation ArchiveProduct($input: ProductInput!) {
            productUpdate(input: $input) {
                product {
                    id
                    status
                }
                userErrors {
                    field
                    message
                }
            }
        }";

                var variables = new
                {
                    input = new
                    {
                        id = $"gid://shopify/Product/{productId}",
                        status = "ARCHIVED"
                    }
                };

                var request = new GraphQLHttpRequest
                {
                    Query = mutation,
                    Variables = variables
                };

                bool retry;
                do
                {
                    retry = false;

                    var response = await _client.SendMutationAsync<dynamic>(request);

                    if (response.Errors != null && response.Errors.Any())
                    {
                        throw new Exception($"GraphQL Error: {string.Join(", ", response.Errors.Select(e => e.Message))}");
                    }

                    var extensionsDict = response.Extensions as IDictionary<string, object>;
                    if (extensionsDict != null && extensionsDict.ContainsKey("cost"))
                    {
                        var costDict = extensionsDict["cost"] as IDictionary<string, object>;
                        var throttleStatusDict = costDict["throttleStatus"] as IDictionary<string, object>;

                        int requestedQueryCost = Convert.ToInt32(costDict["requestedQueryCost"]);
                        int availableBudget = Convert.ToInt32(throttleStatusDict["currentlyAvailable"]);

                        if (requestedQueryCost > availableBudget)
                        {
                            retry = true;
                            Console.WriteLine("Throttling detected. Retrying in 1 second...");
                            await Task.Delay(1000);
                        }
                    }
                } while (retry);
            }
        }

        public async Task CreateProductsAsync(List<ShopifySharp.Product> products)
        {
            foreach (var product in products)
            {
                await _limiter.PerformAsync(async () =>
                {
                    await _service.CreateAsync(product);
                    Console.WriteLine($"Created Product: {product.Variants.FirstOrDefault()?.SKU}");
                });
            }
        }

        public async Task UpdateProductsTagsAsync(List<ShopifySharp.Product> products)
        {
            int count = products.Count();
            foreach (var product in products)
            {
                Console.WriteLine(count--);
                string mutation = @"
            mutation UpdateProductTags($input: ProductInput!) {
                productUpdate(input: $input) {
                    product {
                        id
                        tags
                    }
                    userErrors {
                        field
                        message
                    }
                }
            }";

                var variables = new
                {
                    input = new
                    {
                        id = $"gid://shopify/Product/{product.Id}",
                        tags = product.Tags
                    }
                };

                var request = new GraphQLHttpRequest
                {
                    Query = mutation,
                    Variables = variables
                };

                bool retry;
                do
                {
                    retry = false;

                    var response = await _client.SendMutationAsync<dynamic>(request);

                    // Check for errors in the mutation response
                    if (response.Errors != null && response.Errors.Any())
                    {
                        throw new Exception($"GraphQL Error: {string.Join(", ", response.Errors.Select(e => e.Message))}");
                    }

                    // Extract cost information from the extensions field
                    var extensionsDict = response.Extensions as IDictionary<string, object>;
                    if (extensionsDict != null && extensionsDict.ContainsKey("cost"))
                    {
                        // Convert the "cost" object to a dictionary
                        var costDict = extensionsDict["cost"] as IDictionary<string, object>;
                        var throttleStatusDict = costDict["throttleStatus"] as IDictionary<string, object>;

                        int requestedQueryCost = Convert.ToInt32(costDict["requestedQueryCost"]);
                        int availableBudget = Convert.ToInt32(throttleStatusDict["currentlyAvailable"]);

                        // Check if the cost exceeds the available budget
                        if (requestedQueryCost > availableBudget)
                        {
                            retry = true;
                            await Task.Delay(1000); // Wait for 1 second
                        }
                    }
                } while (retry);
            }
        }


        public async Task<List<Classes.GQLObjects.Product>> FindProductsBySkuAsync(string sku)
        {
            string query = @"
                query($sku: String!) {
                    products(first: 50, query: $sku) {
                        edges {
                            node {
                                id
                                tags
                                title
                                variants(first: 50) {
                                    edges {
                                        node {
                                            id
                                            sku
                                        }
                                    }
                                }
                            }
                        }
                    }
                }";

            var variables = new { sku = $"sku:{sku}" };

            var response = await _client.SendQueryAsync<ProductsBySkuResponse>(query, variables);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception("GraphQL query failed: " + string.Join(", ", response.Errors.Select(e => e.Message)));
            }

            return response.Data.Products.Edges
                .Where(edge => edge.Node.Variants.Edges.Any(variantEdge => variantEdge.Node.Sku.Contains(sku, StringComparison.OrdinalIgnoreCase)))
                .Select(edge => new Classes.GQLObjects.Product
                {
                    Id = edge.Node.Id,
                    Title = edge.Node.Title,
                    Tags = edge.Node.Tags,
                    Variants = edge.Node.Variants.Edges
                        .Select(variantEdge => new Classes.GQLObjects.ProductVariant
                        {
                            Id = variantEdge.Node.Id,
                            Sku = variantEdge.Node.Sku
                        })
                        .ToList()
                })
                .ToList();
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
