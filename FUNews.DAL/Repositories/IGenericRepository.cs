using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FUNews.DAL.Repositories
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity?> FindById(TKey id, string idPropertyName, params Expression<Func<TEntity, object>>[] includeProperties);

        void Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }

}
