using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BA.OrderScraper.Models.Syspro.Api
{
    /// <summary>
    /// Response model for Syspro Sales Order API operations
    /// </summary>
    [XmlRoot("PostSalesOrderResponse")]
    public class SalesOrderResponse
    {
        [XmlElement("StatusOfItems")]
        public StatusOfItems StatusOfItems { get; set; }

        [XmlElement("ErrorNumbers")]
        public ErrorNumbers ErrorNumbers { get; set; }

        [XmlElement("Warnings")]
        public Warnings Warnings { get; set; }

        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess => StatusOfItems?.ItemsProcessed?.Count > 0 && 
                                 (ErrorNumbers?.ErrorNumber?.Count == 0 || ErrorNumbers?.ErrorNumber == null);

        /// <summary>
        /// Gets the created sales order number if successful
        /// </summary>
        public string GetOrderNumber()
        {
            return StatusOfItems?.ItemsProcessed?.FirstOrDefault()?.SalesOrder;
        }

        /// <summary>
        /// Gets all error messages as a concatenated string
        /// </summary>
        public string GetErrorMessage()
        {
            if (ErrorNumbers?.ErrorNumber != null && ErrorNumbers.ErrorNumber.Count > 0)
            {
                return string.Join("; ", ErrorNumbers.ErrorNumber.Select(e => e.Description));
            }
            return string.Empty;
        }
    }

    public class StatusOfItems
    {
        [XmlElement("ItemProcessed")]
        public List<ItemProcessed> ItemsProcessed { get; set; } = new List<ItemProcessed>();
    }

    public class ItemProcessed
    {
        [XmlElement("SalesOrder")]
        public string SalesOrder { get; set; }

        [XmlElement("Customer")]
        public string Customer { get; set; }

        [XmlElement("CustomerName")]
        public string CustomerName { get; set; }
    }

    public class ErrorNumbers
    {
        [XmlElement("ErrorNumber")]
        public List<ErrorNumber> ErrorNumber { get; set; } = new List<ErrorNumber>();
    }

    public class ErrorNumber
    {
        [XmlElement("Number")]
        public string Number { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
    }

    public class Warnings
    {
        [XmlElement("SystemInformation")]
        public string SystemInformation { get; set; }
    }
}