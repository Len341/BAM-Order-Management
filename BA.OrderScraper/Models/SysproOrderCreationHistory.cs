using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models
{
    public class SysproOrderCreationHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = new Guid();
        public int ManifestNumber { get; set; }
        public bool CreationSuccess { get; set; }
        public bool InProgress { get; set; }
        public string? OrderNumber { get; set; }
        public int OrderTotalItems { get; set; }
        public int OrderTotalItemsCompleted { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
