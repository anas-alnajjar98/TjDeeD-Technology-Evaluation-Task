using Application.DTOs.ProductDTOs;
using Application.Interfaces;
using Domain.Entities.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="createProductDto">The product data to be created.</param>
    /// <returns>A success message if the product is created, otherwise returns an error message.</returns>
    /// <response code="200">Returns a success message when the product is created successfully.</response>
    /// <response code="400">Returns a bad request if the image file is not provided.</response>
    /// <response code="500">Returns an internal server error if an unexpected error occurs.</response>

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto)
    {
        try
        {
            if (createProductDto.Image == null || createProductDto.Image.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            await _productService.AddProductAsync(createProductDto);
            return Ok("Product creaeted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the product.");
            return StatusCode(500, ex.Message);
        }
    }


    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>The product details if found, otherwise returns a 404 Not Found.</returns>
    /// <response code="200">Returns the product details if the product is found.</response>
    /// <response code="404">If no product is found with the provided ID.</response>
    /// <response code="500">Internal server error if an unexpected error occurs during the process.</response>
    [HttpGet("GetProductByID/{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                return NotFound($"Product with ID {id} not found.");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching product with ID {ProductId}", id);
            return StatusCode(500, "Internal server error. Please try again later.");
        }
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to be deleted.</param>
    /// <returns>No content if the product is deleted successfully, or an error message if an issue occurs.</returns>
    /// <response code="204">Returns no content when the product is deleted successfully.</response>
    /// <response code="401">Returns unauthorized if the user is not authenticated or does not have the "Admin" role.</response>
    /// <response code="404">Returns not found if the product with the given ID does not exist.</response>
    /// <response code="500">Returns an internal server error if an unexpected error occurs.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    /// <summary>
    /// Updates a product's details by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to be updated.</param>
    /// <param name="updateProductDto">The updated product details.</param>
    /// <returns>No content if the product is updated successfully, or an error message if an issue occurs.</returns>
    /// <response code="204">Returns no content when the product is updated successfully.</response>
    /// <response code="401">Returns unauthorized if the user is not authenticated or does not have the "Admin" role.</response>
    /// <response code="404">Returns not found if the product with the given ID does not exist.</response>
    /// <response code="500">Returns an internal server error if an unexpected error occurs.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
    {
        try
        {
            await _productService.UpdateProductAsync(id, updateProductDto);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
