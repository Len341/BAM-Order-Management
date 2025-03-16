using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models
{
    public class Manifest
    {
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostCode { get; set; }
        public string SupplierNumber { get; set; }
        public DateTime ShipDate { get; set; }
        public TimeSpan ShipTime { get; set; }
        public string PickupRoute { get; set; }
        public string SupplierRoute { get; set; }
        public string SupplierArriveDock { get; set; }
        public string SupplierDepartDock { get; set; }
        public string JHB_PE { get; set; }
        public string SupplierArriveDocJHBPE { get; set; }
        public string SupplierDepartDockJHBPE { get; set; }
        public string SupplierOnDockRoute { get; set; }
        public DateTime SupplierArrivalDateParsed { get; set; }
        public TimeSpan SupplierArrivalTime { get; set; }
        public string SupplierProglane { get; set; }
        public string SupplierShop { get; set; }
        public int SupplierManifestNo { get; set; }
        public string SupplierReceivingDock { get; set; }
        public string SupplierOrderNumber { get; set; }
        public string SupplierKanbanNumber { get; set; }
        public string SupplierMaterialNumber { get; set; }
        public string SupplierPadEasyReferenceNumber { get; set; }
        public bool SupplierPadEasyReferenceNumberNotFound { get; set; }
        public string SupplierPadBinTypeCode { get; set; }
        public long SupplierQty { get; set; }
        public string SupplierPurchasingDocumentNumber { get; set; }
        public string SupplierBinReq { get; set; }
        public DateTime ImportTime { get; set; } = DateTime.Now;
    }
}
