using ClassLibrary.Services.Implementations;
using ShopifySharp;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhatWeMissed
{
    internal class Program
    {
        private static ExcelWriterService _excelWriterService;
        private static CustomProductService _prodService;

        static async Task Main(string[] args)
        {
            try
            {
                ConfigurationService config = new ConfigurationService("appsettings.json");
                _excelWriterService = new ExcelWriterService();
                _prodService = new CustomProductService(config.GetShopifySettings());
                string filePath = config.GetValue("FilePaths:StockExcelLocation");
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                Console.WriteLine("Fetching all products...");
                List<Product> prodList = await _prodService.FetchAllProductsAsync();

                Console.WriteLine("Filtering products without required tags...");
                var missedList = prodList.Where(product =>
                    !HasRequiredTags(product.Tags, new[] { "STOCKProduct", "MTOProduct" })
                ).ToList();

                Console.WriteLine("Filtering inactive products...");
                missedList = missedList.Where(product =>
                    product.Status.Equals("active", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Console.WriteLine("Filtering products with minimum quantity > 110...");
                missedList = missedList.Where(product =>
                    !HasMinimumQuantityExceeding(product.Tags, 110)
                ).ToList();

                Console.WriteLine("Filtering panels...");
                missedList = missedList.Where(product =>
                    !product.Handle.Contains("panel", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Console.WriteLine("Filtering Small Batch...");
                missedList = missedList.Where(product =>
                    !HasRequiredTags(product.Tags, new[] { "SampleProduct" })
                ).ToList();

                Console.WriteLine("Mapping missed products...");
                List<MissedProduct> finalMissedList = missedList
                    .Select(product => MapToMissedProduct(product))
                    .ToList();

                Console.WriteLine($"Writing missed products to {filePath}...");
                _excelWriterService.WriteExcelFile(filePath, finalMissedList);

                Console.WriteLine("Archiving products...");
                var productIdsToArchive = missedList.Select(product => product.Id.Value).ToList();
                await _prodService.ArchiveProductsAsync(productIdsToArchive);

                Console.WriteLine("Process completed successfully!");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static bool HasRequiredTags(string tags, string[] requiredTags)
        {
            if (string.IsNullOrWhiteSpace(tags)) return false;

            var tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(tag => tag.Trim())
                               .ToArray();
            return tagArray.Intersect(requiredTags, StringComparer.OrdinalIgnoreCase).Any();
        }

        private static bool HasMinimumQuantityExceeding(string tags, int threshold)
        {
            if (string.IsNullOrWhiteSpace(tags)) return false;

            var tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(tag => tag.Trim())
                               .ToArray();

            string minQuantTag = tagArray.FirstOrDefault(tag =>
                tag.StartsWith("Minimum Quantity", StringComparison.OrdinalIgnoreCase));

            if (minQuantTag != null)
            {
                var match = Regex.Match(minQuantTag, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int minQuantity))
                {
                    return minQuantity > threshold;
                }
            }

            return false;
        }

        private static MissedProduct MapToMissedProduct(Product product)
        {
            string[] tagArray = product.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(tag => tag.Trim())
                                           .ToArray() ?? Array.Empty<string>();

            string targetTag = tagArray.FirstOrDefault(t =>
                t.StartsWith("Minimum Quantity", StringComparison.OrdinalIgnoreCase));

            return new MissedProduct
            {
                Id = product.Id.Value,
                Handle = product.Handle,
                Tags = product.Tags,
                CurrentMinQuant = targetTag ?? "N/A",
                SKU = product.Variants.FirstOrDefault()?.SKU ?? "N/A"
            };
        }

        // Models
        public class MissedProduct
        {
            public long Id { get; set; }
            public string SKU { get; set; }
            public string Handle { get; set; }
            public string CurrentMinQuant { get; set; }
            public string Tags { get; set; }
        }
    }
}