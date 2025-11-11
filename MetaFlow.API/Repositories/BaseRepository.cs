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

        protected async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
            IQueryable<T> query, 
            int pageNumber, 
            int pageSize)
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
            return ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }
    }
}