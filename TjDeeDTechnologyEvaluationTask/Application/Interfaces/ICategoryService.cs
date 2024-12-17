using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<int> CreateCategoryAsync(string categoryName);
        void UpdateCategory(int categoryId,string updatedCategory);
        Task DeleteCategoryAsync(int categoryId);
    }
}
