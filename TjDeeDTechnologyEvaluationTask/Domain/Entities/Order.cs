using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } 
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";  
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

       
        public List<Payment> Payments { get; set; } = new List<Payment>(); 
    }
}
