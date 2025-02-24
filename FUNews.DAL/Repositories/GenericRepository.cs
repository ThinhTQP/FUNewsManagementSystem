using BusinessObjects.Entities;
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

  
    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> items = _context.Set<TEntity>();

        if (includeProperties.Any()) 
        {
            foreach (var property in includeProperties)
            {
                items = items.Include(property);
            }
        }

    
        if (predicate != null)
        {
            items = items.Where(predicate);
        }

        return items;
    }

   
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




    public void Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }


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

        return "1";
    }
}
