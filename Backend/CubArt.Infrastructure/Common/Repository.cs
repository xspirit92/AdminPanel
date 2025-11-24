using CubArt.Domain.Common;
using CubArt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Common
{
    // Базовый репозиторий
    public interface IRepository<T, TId> where T : Entity<TId> where TId : notnull
    {
        Task<T?> GetByIdAsync(TId id, bool includeRelated = false);
        IQueryable<T> GetQueryable();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }

    public class Repository<T, TId> : IRepository<T, TId> where T : Entity<TId> where TId : notnull
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(TId id, bool includeRelated = false)
        {
            var query = _dbSet.AsNoTracking();

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsNoTracking().AsQueryable();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
