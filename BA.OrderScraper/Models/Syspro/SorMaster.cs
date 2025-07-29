using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models.Syspro
{
    public class SorMaster
    {
        [Key]
        public string SalesOrder { get; set; }
        public string CustomerPoNumber { get; set; }
    }
}
