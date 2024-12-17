using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; } 
        public int OrderId { get; set; } 
        public decimal Amount { get; set; } 
        public string PaymentIntentId { get; set; } 
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } 
      
        public Order Order { get; set; } 
    }
}
