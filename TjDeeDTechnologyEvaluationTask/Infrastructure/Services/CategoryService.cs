using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service for managing categories, including creating, updating, and deleting categories.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly DbContextTask _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="context">The database context for interacting with the Categories and Products tables.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for retrieving the current user's information.</param>
        /// <param name="logger">The logger for logging events during category operations.</param>
        public CategoryService(DbContextTask context, IHttpContextAccessor httpContextAccessor, ILogger<CategoryService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new category with the specified name.
        /// </summary>
        /// <param name="categoryName">The name of the category to be created.</param>
        /// <returns>The ID of the created category.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated or does not have the necessary permissions.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during category creation.</exception>
        public async Task<int> CreateCategoryAsync(string categoryName)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext.User;

                if (user == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Unauthorized access attempt while creating a category.");
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }

                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != "Admin")
                {
                    _logger.LogWarning("Permission denied. User with role '{Role}' attempted to create a category.", userRole);
                    throw new UnauthorizedAccessException("User does not have permission to create a category.");
                }

                var category = new Domain.Entities.Category
                {
                    CategoryName = categoryName
                };

                _logger.LogInformation("Attempting to create a category with name '{CategoryName}' by user '{User}'.", categoryName, user.Identity.Name);

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}.", categoryName, category.CategoryId);

                return category.CategoryId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a category.");
                throw;
            }
        }

        /// <summary>
        /// Updates the name of an existing category.
        /// </summary>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="updatedCategory">The new name for the category.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the category with the specified ID does not exist.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during category update.</exception>
        public void UpdateCategory(int categoryId, string updatedCategory)
        {
            try
            {
                _logger.LogInformation("Attempting to update category with ID {CategoryId}.", categoryId);

                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", categoryId);
                    throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
                }

                _logger.LogInformation("Updating category. Old Name: '{OldName}', New Name: '{NewName}'.", category.CategoryName, updatedCategory);

                category.CategoryName = updatedCategory;

                _context.Categories.Update(category);
                _context.SaveChanges();

                _logger.LogInformation("Category with ID {CategoryId} updated successfully.", categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID {CategoryId}.", categoryId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a category and its associated products from the database.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated or does not have the necessary permissions.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the category with the specified ID does not exist.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during category deletion.</exception>
        public async Task DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext.User;

                if (user == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Unauthorized access attempt while deleting a category.");
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }

                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != "Admin")
                {
                    _logger.LogWarning("Permission denied. User with role '{Role}' attempted to delete a category.", userRole);
                    throw new UnauthorizedAccessException("User does not have permission to delete a category.");
                }

                var category = await _context.Categories
                    .Include(c => c.Products)  
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", categoryId);
                    throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
                }

                _context.Products.RemoveRange(category.Products);  
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category with ID {CategoryId} and its associated products were successfully deleted.", categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID {CategoryId}.", categoryId);
                throw;
            }
        }
    }

}
