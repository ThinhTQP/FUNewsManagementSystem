using BusinessObjects.Entities;
using FUNews.DAL.Repositories;
using FUNews.BLL.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FUNews.BLL.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Tag, int> _tagRepository;

        public TagService(IGenericRepository<Tag, int> tagRepository, IUnitOfWork unitOfWork)
        {
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
        }

        // Get all tags
        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.FindAll().ToListAsync();
        }

        // Get tag by ID
        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _tagRepository.FindById(id, "TagId");
        }

        // Add a new tag
        public async Task AddTagAsync(Tag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            _tagRepository.Create(tag);
            await _unitOfWork.SaveChange(); 
        }

        // Update an existing tag
        public async Task UpdateTagAsync(Tag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            _tagRepository.Update(tag);
            await _unitOfWork.SaveChange(); 
        }

        // Delete a tag by ID
        public async Task DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.FindById(id, "TagId", c => c.NewsArticles);
            if (tag == null)
                throw new ArgumentException("Tag not found");
            if (tag.NewsArticles != null && tag.NewsArticles.Any())
                throw new InvalidOperationException("Cannot delete this category because it is associated with one or more news articles.");
            _tagRepository.Delete(tag);
            await _unitOfWork.SaveChange(); 
        }
    }
}
