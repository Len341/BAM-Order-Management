using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models
{
    public class Skid
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ReceivingDock { get; set; }
        public string ProgressLane { get; set; }
        public string OrderNumber { get; set; }
        public string PickupRoute { get; set; }
        public DateTime DepartDate { get; set; }
        public string Route { get; set; }
        public DateTime XDoc2ArrivalDate { get; set; }
        public TimeSpan XDoc2ArrivalTime { get; set; }
        public DateTime DepartDate2 { get; set; }
        public DateTime DepartDate3 { get; set; }
        public TimeSpan DepartTime { get; set; }
        public string OnDockRoute { get; set; }
        public DateTime ArrivalDate { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public DateTime TsamDepartDocDate { get; set; }
        public TimeSpan TsamDepartDocTime { get; set; }
        public string ManifestNumber { get; set; }
    }
}
