using System.ComponentModel.DataAnnotations;

namespace BA.OrderScraper.Models.Syspro
{
    public class InvMaster
    {
        [Key]
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string AlternateKey1 { get; set; }
        public string WarehouseToUse { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    public class SorMaster
    {
        [Key]
        public string SalesOrder { get; set; }
        public string Customer { get; set; }
        public string CustomerPoNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ReqShipDate { get; set; }
    }
}