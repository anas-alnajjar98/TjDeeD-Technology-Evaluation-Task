using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.ProductDTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task AddProductAsync(CreateProductDto createProductDto);
        Task<Product> GetProductByIdAsync(int productId);
        Task DeleteProductAsync(int productId);
        Task UpdateProductAsync(int productId, UpdateProductDto updateProductDto);


    }
}
