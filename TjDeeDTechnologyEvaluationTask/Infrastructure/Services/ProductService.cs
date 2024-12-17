using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.ProductDTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class ProductService:IProductService
    {
        private readonly DbContextTask _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductService> _logger;
       private readonly IImageService _imageService;
        public ProductService(DbContextTask context, IHttpContextAccessor httpContextAccessor, ILogger<ProductService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="createProductDto">The data transfer object containing the product information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated or does not have the "Admin" role.</exception>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while adding the product to the database.</exception>
        public async Task AddProductAsync(CreateProductDto createProductDto)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to add product.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                _logger.LogWarning("Permission denied. User with role '{Role}' attempted to add a product.", userRole);
                throw new UnauthorizedAccessException("User does not have permission to add a product.");
            }

            var product = new Product
            {
                ProductName = createProductDto.ProductName,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                CategoryId = createProductDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ImageUrl = createProductDto.Image.ToString()
            };

            //if (createProductDto.Image != null && createProductDto.Image.Length > 0)
            //{
            //    try
            //    {
            //        var imageUrl = await _imageService.SaveImageAsync(createProductDto.Image);
            //        product.ImageUrl = imageUrl;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error occurred while saving the image for product '{ProductName}'.", product.ProductName);
            //        throw new Exception("There was an error saving the image.");
            //    }
            //}

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product '{ProductName}' added successfully with ID {ProductId}.", product.ProductName, product.ProductId);
        }



        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve.</param>
        /// <returns>The product if found, otherwise null.</returns>
        /// <exception cref="Exception">Throws if an error occurs while retrieving the product.</exception>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                return null;  // Return null if not found instead of throwing an exception
            }

            return product;
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated or does not have the "Admin" role.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the product with the specified ID does not exist.</exception>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while deleting the product.</exception>
        public async Task DeleteProductAsync(int productId)
        {

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to delete product.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                _logger.LogWarning("Permission denied. User with role '{Role}' attempted to delete a product.", userRole);
                throw new UnauthorizedAccessException("User does not have permission to delete a product.");
            }


            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }


            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product with ID {ProductId} deleted successfully.", productId);
        }
        /// <summary>
        /// Updates a product's details by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to be updated.</param>
        /// <param name="updateProductDto">The updated product details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated or does not have the "Admin" role.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the product with the specified ID does not exist.</exception>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while updating the product.</exception>
        public async Task UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to update product.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                _logger.LogWarning("Permission denied. User with role '{Role}' attempted to update a product.", userRole);
                throw new UnauthorizedAccessException("User does not have permission to update a product.");
            }

           
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }


            product.ProductName = string.IsNullOrEmpty(updateProductDto.ProductName) || updateProductDto.ProductName == "string"
                ? product.ProductName
                : updateProductDto.ProductName;
            product.Description = string.IsNullOrEmpty(updateProductDto.Description) || updateProductDto.Description == "string"
        ? product.Description
        : updateProductDto.Description;

            product.Price = updateProductDto.Price.HasValue && updateProductDto.Price.Value > 0
                ? updateProductDto.Price.Value
                : product.Price;

            product.CategoryId = updateProductDto.CategoryId.HasValue && updateProductDto.CategoryId.Value > 0
                ? updateProductDto.CategoryId.Value
                : product.CategoryId;

            product.ImageUrl = string.IsNullOrEmpty(updateProductDto.Image) || updateProductDto.Image == "string"
                ? product.ImageUrl
                : updateProductDto.Image;


            // Update the image if provided
            //if (image != null && image.Length > 0)
            //{
            //    try
            //    {
            //        var imageUrl = await _imageService.SaveImageAsync(image);
            //        product.ImageUrl = imageUrl;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error occurred while saving the image for product '{ProductName}'.", product.ProductName);
            //        throw new Exception("There was an error saving the image.");
            //    }
            //}

            product.UpdatedAt = DateTime.UtcNow;

            // Save changes
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product with ID {ProductId} updated successfully.", productId);
        }
    }
}
