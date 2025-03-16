using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models
{
    public class KanbanCard
    {
        public string SupplierName { get; set; }
        public string SupplierNumber { get; set; }
        public string SupplierPartNumber { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string Shop { get; set; }
        public string PartName { get; set; }
        public string MaterialNumber { get; set; }
        public string KanbanNo { get; set; }
        public string OrderNumber { get; set; }
        public string BinTypeCode { get; set; }
        public int Qty { get; set; }
        public string KanbanPrintAddress1 { get; set; }
        public string ReceivingDock { get; set; }
        public string ProgressLane { get; set; }
        [Key]
        public string KanbanID { get; set; }
        public string OnDockRoute { get; set; }
        public string ManifestNumber { get; set; }
    }
}
