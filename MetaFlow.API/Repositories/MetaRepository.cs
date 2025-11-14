using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;
using System.Linq.Expressions;

namespace MetaFlow.API.Repositories
{
    public interface IMetaRepository
    {
        Task<(List<Meta> Metas, int TotalCount)> GetPagedAsync(
            Expression<Func<Meta, bool>>? filter = null,
            PaginationParams? paginationParams = null,
            Expression<Func<Meta, object>>? orderBy = null,
            bool ascending = true);

        Task<Meta?> GetByIdAsync(Guid id, bool includeUsuario = false);
        Task<List<Meta>> GetByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null);
        Task<Meta?> GetByUsuarioAndIdAsync(Guid usuarioId, Guid metaId);
        Task<int> CountByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null);
        Task<Dictionary<StatusMeta, int>> GetEstatisticasStatusAsync(Guid usuarioId);
        Task<List<Meta>> GetMetasProximasDoPrazoAsync(int dias = 7);
        Task<bool> ExistsAsync(Guid id);
        Task AddAsync(Meta meta);
        Task UpdateAsync(Meta meta);
        Task DeleteAsync(Meta meta);
        
        Task<int> GetTotalMetasAsync();
        Task<int> GetTotalMetasConcluidasAsync();
        Task<int> GetNovasMetasUltimos7DiasAsync();
        Task<List<Meta>> GetMetasRecentesAsync(int quantidade = 10);
        Task<Dictionary<string, int>> GetDistribuicaoCategoriasAsync();
    }

    public class MetaRepository : BaseRepository<Meta>, IMetaRepository
    {
        public MetaRepository(MetaFlowDbContext context) : base(context) { }

        public async Task<(List<Meta> Metas, int TotalCount)> GetPagedAsync(
            Expression<Func<Meta, bool>>? filter = null,
            PaginationParams? paginationParams = null,
            Expression<Func<Meta, object>>? orderBy = null,
            bool ascending = true)
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            var orderExpression = orderBy ?? (m => m.Prazo);
            query = ascending ? 
                query.OrderBy(orderExpression) : 
                query.OrderByDescending(orderExpression);

            paginationParams ??= new PaginationParams { PageNumber = 1, PageSize = 20 };
            
            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<Meta?> GetByIdAsync(Guid id, bool includeUsuario = false)
        {
            var query = _dbSet.AsQueryable();
            
            if (includeUsuario)
                query = query.Include(m => m.Usuario);

            return await query.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Meta>> GetByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null)
        {
            var query = _dbSet.Where(m => m.UsuarioId == usuarioId);

            if (filter != null)
                query = query.Where(filter);

            return await query
                .OrderBy(m => m.Prazo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Meta?> GetByUsuarioAndIdAsync(Guid usuarioId, Guid metaId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);
        }

        public async Task<int> CountByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null)
        {
            var query = _dbSet.Where(m => m.UsuarioId == usuarioId);

            if (filter != null)
                query = query.Where(filter);

            return await query.CountAsync();
        }

        public async Task<Dictionary<StatusMeta, int>> GetEstatisticasStatusAsync(Guid usuarioId)
        {
            var estatisticas = await _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return estatisticas.ToDictionary(
                x => x.Status,
                x => x.Count
            );
        }

        public async Task<List<Meta>> GetMetasProximasDoPrazoAsync(int dias = 7)
        {
            var dataLimite = DateTime.UtcNow.AddDays(dias);
            return await _dbSet
                .Where(m => m.Prazo <= dataLimite && 
                           m.Prazo >= DateTime.UtcNow &&
                           m.Status == StatusMeta.Ativa)
                .Include(m => m.Usuario)
                .OrderBy(m => m.Prazo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(m => m.Id == id);
        }

        public async Task AddAsync(Meta meta)
        {
            await _dbSet.AddAsync(meta);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Meta meta)
        {
            _dbSet.Update(meta);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Meta meta)
        {
            _dbSet.Remove(meta);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalMetasAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> GetTotalMetasConcluidasAsync()
        {
            return await _dbSet.CountAsync(m => m.Status == StatusMeta.Concluida);
        }

        public async Task<int> GetNovasMetasUltimos7DiasAsync()
        {
            var dataLimite = DateTime.UtcNow.AddDays(-7);
            return await _dbSet.CountAsync(m => m.CriadoEm >= dataLimite);
        }

        public async Task<List<Meta>> GetMetasRecentesAsync(int quantidade = 10)
        {
            return await _dbSet
                .OrderByDescending(m => m.CriadoEm)
                .Take(quantidade)
                .Include(m => m.Usuario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetDistribuicaoCategoriasAsync()
        {
            var distribuicao = await _dbSet
                .GroupBy(m => m.Categoria)
                .Select(g => new { Categoria = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return distribuicao.ToDictionary(x => x.Categoria, x => x.Count);
        }
    }
}