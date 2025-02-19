using FUNews.DAL.Entities;
using FUNews.DAL.Repositories;
using FUNews.BLL.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FUNews.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Category, short> _categoryRepository;

        // Constructor 
        public CategoryService(IGenericRepository<Category, short> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        // Lấy tất cả các Category
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.FindAll().ToListAsync();
        }

        // Lấy Category theo ID
        public async Task<Category?> GetCategoryByIdAsync(short id)
        {
            return await _categoryRepository.FindById(id, "CategoryId", c => c.ParentCategory);
        }

        // Thêm mới Category
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Create(category);
            await _unitOfWork.SaveChange();
        }

        // Cập nhật Category
        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChange(); 
        }

        // Xóa Category
        public async Task DeleteCategoryAsync(short id)
        {
            var category = await _categoryRepository.FindById(id, "CategoryId", c => c.NewsArticles);
            if (category == null)
                throw new ArgumentException("Category not found");
            if (category.NewsArticles != null && category.NewsArticles.Any())
                throw new InvalidOperationException("Cannot delete this category because it is associated with one or more news articles.");
            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChange();
        }

        
        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository
                .FindAll(c => c.IsActive == true) 
                .ToListAsync();
        }

    }
}
