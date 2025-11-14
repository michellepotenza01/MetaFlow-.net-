using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models.Common;
using System.Linq.Expressions;

namespace MetaFlow.API.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly MetaFlowDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(MetaFlowDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query)
        {
            return query;
        }

        protected async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
            IQueryable<T> query, 
            int pageNumber = 1, 
            int pageSize = 20)
        {
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        protected IQueryable<T> ApplyOrdering<TKey>(
            IQueryable<T> query, 
            Expression<Func<T, TKey>> orderBy, 
            bool ascending = true)
        {
            return ascending ? 
                query.OrderBy(orderBy) : 
                query.OrderByDescending(orderBy);
        }

        protected IQueryable<T> ApplySpecification(
            IQueryable<T> query,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return query;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }
    }
}