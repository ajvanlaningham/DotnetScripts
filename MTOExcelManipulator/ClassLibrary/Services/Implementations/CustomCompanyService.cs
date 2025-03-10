using ClassLibrary.Classes;
using ClassLibrary.Services.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;


namespace ClassLibrary.Services.Implementations
{
    public class CustomCompanyService : ICustomCompanyService
    {
        private readonly IGraphQLClient _client;
        private readonly RateLimiter _limiter;

        public CustomCompanyService(ShopifyAdminAPISettings settings)
        {
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

        public async Task<string> GetSiteID(long companyId, long locationId)
        {
            var query = new GraphQLRequest
            {
                Query = @"
        query GetCompanyLocations($companyID: ID!) {
          company(id: $companyID) {
            locations(first: 10) {
              edges {
                node {
                  id
                  metafields(first: 10) {
                    edges {
                      node {
                        namespace
                        key
                        value
                      }
                    }
                  }
                }
              }
            }
          }
        }",
                Variables = new { companyID = $"gid://shopify/Company/{companyId}" }
            };

            var response = await _client.SendQueryAsync<CompanyResponse>(query);
            var globalIdSuffix = $"/{locationId}";

            if (response.Data?.Company?.Locations?.Edges != null)
            {
                var matchingLocation = response.Data.Company.Locations.Edges
                    .FirstOrDefault(edge => edge.Node.Id.EndsWith(globalIdSuffix));
                if (matchingLocation != null && matchingLocation.Node.Metafields?.Edges != null)
                {
                    var metafield = matchingLocation.Node.Metafields.Edges
                        .FirstOrDefault(m => m.Node.Namespace == "custom" && m.Node.Key == "shiptoid");
                    if (metafield != null && !string.IsNullOrEmpty(metafield.Node.Value))
                    {
                        return metafield.Node.Value;
                    }
                }
            }

            throw new Exception("ShipToID not found for the given company and location ID.");
        }

        // GraphQL Response Types
        public class CompanyResponse
        {
            public Company Company { get; set; }
        }

        public class Company
        {
            public LocationConnection Locations { get; set; }
        }

        public class LocationConnection
        {
            public List<LocationEdge> Edges { get; set; }
        }

        public class LocationEdge
        {
            public Location Node { get; set; }
        }

        public class Location
        {
            public string Id { get; set; }
            public MetafieldConnection Metafields { get; set; }
        }

        public class MetafieldConnection
        {
            public List<MetafieldEdge> Edges { get; set; }
        }

        public class MetafieldEdge
        {
            public Metafield Node { get; set; }
        }

        public class Metafield
        {
            public string Namespace { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
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
