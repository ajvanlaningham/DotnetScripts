using ShopifySharp;

namespace ClassLibrary.Services.Interfaces
{
    public interface ICustomProductService
    {
        Task<List<Product>> FetchAllProductsAsync();
        Task ArchiveProductsAsync(IEnumerable<long> productIds);
        Task CreateProductsAsync(List<Product> products);
        Task UpdateProductsTagsAsync(List<Product> products);
    }
}
