using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibrary.Classes.OracleMessages
{
    public class Adjustment
    {
        [JsonPropertyName("cust_account_number")]
        public string CustAccountNumber { get; set; }

        [JsonPropertyName("cust_account_name")]
        public string CustAccountName { get; set; }

        [JsonPropertyName("cust_ship_to_loc")]
        public string CustShipToLoc { get; set; }

        [JsonPropertyName("ship_to_site_number")]
        public string ShipToSiteNumber { get; set; }

        [JsonPropertyName("sbu")]
        public string Sbu { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("price_list_name")]
        public string PriceListName { get; set; }

        [JsonPropertyName("list_price")]
        public decimal ListPrice { get; set; }

        [JsonPropertyName("selling_price")]
        public decimal SellingPrice { get; set; }

        [JsonPropertyName("uom")]
        public string Uom { get; set; }

        [JsonPropertyName("value_from")]
        public decimal? ValueFrom { get; set; }

        [JsonPropertyName("value_to")]
        public decimal? ValueTo { get; set; }

        [JsonPropertyName("surcharge_percent")]
        public decimal? SurchargePercent { get; set; }

        [JsonPropertyName("surcharge_amount")]
        public decimal? SurchargeAmount { get; set; }

        [JsonPropertyName("discount_percent")]
        public decimal? DiscountPercent { get; set; }

        [JsonPropertyName("discount_amount")]
        public decimal? DiscountAmount { get; set; }
    }

    public class AdjustmentList
    {
        [JsonPropertyName("adjustment")]
        public List<Adjustment> Adjustments { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("adjustment_list")]
        public AdjustmentList AdjustmentList { get; set; }
    }
}
