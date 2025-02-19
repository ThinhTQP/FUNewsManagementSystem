using BookApp.Entities;
using FUNews.DAL.Repositories;
using FUNews.DAL;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    private readonly FUNewsManagementContext _context;

    public GenericRepository(FUNewsManagementContext context)
    {
        _context = context;
    }

    // Find all items with an optional predicate and included properties
    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> items = _context.Set<TEntity>();

        // Include any related properties if specified
        if (includeProperties.Any()) // This prevents unnecessary null checks.
        {
            foreach (var property in includeProperties)
            {
                items = items.Include(property);
            }
        }

        // Apply the predicate if specified
        if (predicate != null)
        {
            items = items.Where(predicate);
        }

        return items;
    }

    // Find a specific item by ID with optional related data included
    public async Task<TEntity?> FindById(TKey id, string idPropertyName, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var query = _context.Set<TEntity>().AsQueryable();

        // Thêm Include() với các thuộc tính liên quan
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        // Sử dụng EF.Property để truy xuất giá trị của khóa chính
        return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, idPropertyName).Equals(id));
    }




    // Create a new entity
    public void Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    // Update an existing entity
    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }

    // Delete an entity
    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }
    public async Task<string> GetMaxNewsArticleIdAsync()
    {
        var maxId = await _context.NewsArticles
                                  .OrderByDescending(n => n.NewsArticleId)
                                  .Select(n => n.NewsArticleId)
                                  .FirstOrDefaultAsync();

        if (int.TryParse(maxId, out int currentMaxId))
        {
            return (currentMaxId + 1).ToString();
        }

        // Nếu chưa có dữ liệu, bắt đầu từ 1
        return "1";
    }
}
