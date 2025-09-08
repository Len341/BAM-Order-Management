using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BA.OrderScraper.Models.Syspro.Api
{
    /// <summary>
    /// Request model for Syspro Sales Order creation via API
    /// Based on Syspro SORTOI (Sales Order Transaction Import) business object
    /// </summary>
    [XmlRoot("PostSalesOrder")]
    public class SalesOrderRequest
    {
        [XmlElement("Item")]
        public SalesOrderHeader Item { get; set; }
    }

    public class SalesOrderHeader
    {
        /// <summary>
        /// Customer code - defaults to 'TOY020' based on existing implementation
        /// </summary>
        [XmlElement("Customer")]
        public string Customer { get; set; } = "TOY020";

        /// <summary>
        /// Customer Purchase Order Number (Manifest Number)
        /// </summary>
        [XmlElement("CustomerPoNumber")]
        public string CustomerPoNumber { get; set; }

        /// <summary>
        /// Requested ship date
        /// </summary>
        [XmlElement("ReqShipDate")]
        public string ReqShipDate { get; set; }

        /// <summary>
        /// Shipping method/carrier
        /// </summary>
        [XmlElement("ShippingInstrs")]
        public string ShippingInstrs { get; set; }

        /// <summary>
        /// Order type - typically blank for standard orders
        /// </summary>
        [XmlElement("OrderType")]
        public string OrderType { get; set; } = "";

        /// <summary>
        /// Sales order lines/items
        /// </summary>
        [XmlElement("SalesOrderLine")]
        public List<SalesOrderLine> SalesOrderLines { get; set; } = new List<SalesOrderLine>();
    }

    public class SalesOrderLine
    {
        /// <summary>
        /// Stock code/item number
        /// </summary>
        [XmlElement("StockCode")]
        public string StockCode { get; set; }

        /// <summary>
        /// Warehouse code where stock is located
        /// </summary>
        [XmlElement("Warehouse")]
        public string Warehouse { get; set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        [XmlElement("OrderQty")]
        public decimal OrderQty { get; set; }

        /// <summary>
        /// Unit of measure - defaults to 'EA' (Each)
        /// </summary>
        [XmlElement("OrderUom")]
        public string OrderUom { get; set; } = "EA";

        /// <summary>
        /// Line type - typically blank for stocked items
        /// </summary>
        [XmlElement("LineType")]
        public string LineType { get; set; } = "";
    }
}