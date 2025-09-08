using System.Xml.Serialization;

namespace BA.OrderScraper.Models.Syspro.Api
{
    /// <summary>
    /// Request model for looking up stock codes by alternate key (QuickReference)
    /// Based on Syspro INVQRY (Inventory Query) business object
    /// </summary>
    [XmlRoot("Query")]
    public class StockCodeLookupRequest
    {
        [XmlElement("Key")]
        public StockCodeKey Key { get; set; }
    }

    public class StockCodeKey
    {
        /// <summary>
        /// The alternate key/quick reference to search for
        /// </summary>
        [XmlElement("AlternateKey1")]
        public string AlternateKey1 { get; set; }
    }
}