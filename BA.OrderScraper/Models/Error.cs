using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Models
{
    public class Error
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerExceptionMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? RetryCount { get; set; }
        public Error() { }

        public Error(string errorMessage, string? stackTrace, string? innerExceptionMessage, int retryCount)
        {
            ErrorMessage = errorMessage;
            StackTrace = stackTrace;
            CreatedDate = DateTime.Now;
            RetryCount = retryCount;
            InnerExceptionMessage = innerExceptionMessage;
        }
    }
}
