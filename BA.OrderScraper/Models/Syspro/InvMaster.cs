using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models.Syspro
{
    public class InvMaster
    {
        [Key]
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string AlternateKey1 { get; set; }
        public string AlternateKey2 { get; set; }
        public string WarehouseToUse { get; set; }
    }
}
