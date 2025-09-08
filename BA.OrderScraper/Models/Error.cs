using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BA.OrderScraper.Models
{
    public class Error
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerExceptionMessage { get; set; }
        public int RetryAttempt { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Error() { }

        public Error(string message, string stackTrace, string innerExceptionMessage, int retryAttempt)
        {
            Id = Guid.NewGuid();
            Message = message;
            StackTrace = stackTrace;
            InnerExceptionMessage = innerExceptionMessage;
            RetryAttempt = retryAttempt;
            CreatedDate = DateTime.Now;
        }
    }
}