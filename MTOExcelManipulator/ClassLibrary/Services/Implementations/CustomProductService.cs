using ClassLibrary.Classes;
using ClassLibrary.Services.Implementations;
using ClassLibrary.Services.Interfaces;
using ShopifySharp;
using ShopifySharp.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Implemetations
{
    public class CustomProductService : ICustomProductService
    {
        private readonly ProductService _service;
        private readonly RateLimiter _limiter;

        public CustomProductService(ProductService service, RateLimiter limiter)
        {
            _service = service;
            _limiter = limiter;
        }

        public CustomProductService(ShopifyAdminAPISettings settings)
        {
            _service = new ProductService(settings.StoreUrl, settings.AccessToken);
            _limiter = new RateLimiter(4, TimeSpan.FromSeconds(10));
        }

        public async Task<List<Product>> FetchAllProductsAsync()
        {
            var productList = new List<Product>();
            long? lastId = 0;

            while (lastId >= 0)
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
            foreach (var productId in productIds)
            {
                await _limiter.PerformAsync(async () =>
                {
                    await _service.UpdateAsync(productId, new Product
                    {
                        Status = "archived"
                    });
                    Console.WriteLine($"Archived product with ID: {productId}");
                });
            }
        }

        public async Task CreateProductsAsync(List<Product> products)
        {
            foreach (var product in products)
            {
                await _limiter.PerformAsync(async () =>
                {
                    await _service.CreateAsync(product);

                    Console.WriteLine($"Created Product: {product.Variants.FirstOrDefault().SKU}");
                });
            }
        }

        public async Task UpdateProductsTagsAsync(List<Product> products)
        {
            foreach (var product in products)
            {
                await _limiter.PerformAsync(async () =>
                {
                    await _service.UpdateAsync(product.Id.Value, product);

                    Console.WriteLine($"Created Product: {product.Variants.FirstOrDefault().SKU}");
                });
            }
        }
    }
}
