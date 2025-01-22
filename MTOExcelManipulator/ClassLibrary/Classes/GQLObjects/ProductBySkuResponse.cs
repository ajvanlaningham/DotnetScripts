using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes.GQLObjects
{
    public class ProductsBySkuResponse
    {
        public ProductsData Products { get; set; }
    }

    public class ProductsData
    {
        public List<ProductEdge> Edges { get; set; }
    }

    public class ProductEdge
    {
        public ProductNode Node { get; set; }
    }

    public class ProductNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public VariantsData Variants { get; set; }
    }

    public class VariantsData
    {
        public List<VariantEdge> Edges { get; set; }
    }

    public class VariantEdge
    {
        public VariantNode Node { get; set; }
    }

    public class VariantNode
    {
        public string Id { get; set; }
        public string Sku { get; set; }
    }

    public class Product
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public List<ProductVariant> Variants { get; set; }
    }

    public class ProductVariant
    {
        public string Id { get; set; }
        public string Sku { get; set; }
    }
}
