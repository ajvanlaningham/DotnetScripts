using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes
{
    public class ShopifyAdminAPISettings
    {
        public string StoreUrl { get; set; }
        public string AccessToken { get; set; }

        public static string GetEnvironmentSection(ConfigurationService configService)
        {
            string environment = configService.GetValue("Environment");

            return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
                ? "ShopifySettings"
                : "DevShopifySettings";
        }
    }
}
