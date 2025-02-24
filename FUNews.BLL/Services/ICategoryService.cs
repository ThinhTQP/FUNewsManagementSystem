using System;
using System.Linq;
using System.Text;
using BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FUNews.BLL.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(short id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(short id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    }
}
