using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

       /// <summary>
/// Creates a new category.
/// </summary>
/// <param name="categoryName">The name of the category to create.</param>
/// <returns>Returns a success message with the created category ID.</returns>
/// <response code="200">Category created successfully</response>
/// <response code="400">Bad request due to empty or invalid category name</response>
/// <response code="403">Forbidden if the user does not have the required permissions</response>
[HttpPost]
public async Task<IActionResult> CreateCategory([FromBody] string categoryName)
{
    if (string.IsNullOrWhiteSpace(categoryName))
    {
        return BadRequest("Category name cannot be empty.");
    }

    try
    {
        var categoryId = await _categoryService.CreateCategoryAsync(categoryName);
        return Ok(new { Message = "Category created successfully", CategoryId = categoryId });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Forbid(ex.Message);
    }
}
        /// <summary>
        /// Updates an existing category by its ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="updatedCategory">The updated name for the category.</param>
        /// <returns>Returns a success message if the category was updated, or an error message if something went wrong.</returns>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Bad request due to empty or invalid category name</response>
        /// <response code="404">Not found if the category with the specified ID does not exist</response>
        /// <response code="401">Unauthorized if the user does not have permission to update the category</response>
        /// <response code="500">Internal server error if something goes wrong during the update process</response>
        [HttpPut("{categoryId}")]
        public IActionResult UpdateCategory(int categoryId, [FromBody] string updatedCategory)
        {
            if (updatedCategory == null || string.IsNullOrEmpty(updatedCategory))
            {
                return BadRequest("Category name cannot be empty.");
            }

            try
            {
                _categoryService.UpdateCategory(categoryId, updatedCategory);
                return Ok($"Category with ID {categoryId} updated successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        /// <summary>
        /// Deletes a category and its associated products by the category ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>Returns a success message if the category and its products are deleted, or an error message if something went wrong.</returns>
        /// <response code="200">Category and its products deleted successfully</response>
        /// <response code="400">Bad request if category ID is invalid</response>
        /// <response code="404">Not found if the category with the specified ID does not exist</response>
        /// <response code="401">Unauthorized if the user does not have permission to delete the category</response>
        /// <response code="500">Internal server error if something goes wrong during the deletion process</response>
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(categoryId);
                return Ok($"Category with ID {categoryId} and its products deleted successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
