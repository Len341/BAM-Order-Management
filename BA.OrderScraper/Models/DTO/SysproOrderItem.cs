using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models.DTO
{
    public class SysproOrderItem
    {
        public int CustomerPurchaseOrder { get; set; }
        public DateTime ShipDate { get; set; }
        public string ShipVia { get; set; }
        public bool InProgress { get; set; }
        public string OrderNumber { get; set; }
        public List<SysproOrderLine> Items { get; set; }
    }

    public class SysproOrderLine
    {
        public string QuickReference { get; set; }
        public long Quantity { get; set; }
    }
}
