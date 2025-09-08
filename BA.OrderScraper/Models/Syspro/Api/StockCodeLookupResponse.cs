using System.Collections.Generic;
using System.Xml.Serialization;

namespace BA.OrderScraper.Models.Syspro.Api
{
    /// <summary>
    /// Response model for stock code lookup operations
    /// </summary>
    [XmlRoot("InvQueryResponse")]
    public class StockCodeLookupResponse
    {
        [XmlElement("InventoryItem")]
        public List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

        /// <summary>
        /// Gets the first matching inventory item
        /// </summary>
        public InventoryItem GetFirstItem()
        {
            return InventoryItems?.Count > 0 ? InventoryItems[0] : null;
        }
    }

    public class InventoryItem
    {
        [XmlElement("StockCode")]
        public string StockCode { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("AlternateKey1")]
        public string AlternateKey1 { get; set; }

        [XmlElement("Warehouse")]
        public string Warehouse { get; set; }

        [XmlElement("QtyOnHand")]
        public decimal QtyOnHand { get; set; }

        [XmlElement("Available")]
        public decimal Available { get; set; }

        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }
    }
}