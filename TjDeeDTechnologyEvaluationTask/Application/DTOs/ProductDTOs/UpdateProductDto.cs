using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ProductDTOs
{
    public class UpdateProductDto
    {
        public string ProductName { get; set; } = "string";
        public string Description { get; set; } = "string";
        public decimal? Price { get; set; } 
        public int? CategoryId { get; set; } 
        public string Image { get; set; } = "string";
    }
}
