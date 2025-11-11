using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;

namespace MetaFlow.API.Repositories
{
    public interface IMetaRepository
    {
        Task<(List<Meta> Metas, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams);
        Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioAndStatusPagedAsync(Guid usuarioId, StatusMeta status, PaginationParams paginationParams);
        Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioAndCategoriaPagedAsync(Guid usuarioId, CategoriaMeta categoria, PaginationParams paginationParams);
        Task<List<Meta>> GetByUsuarioAsync(Guid usuarioId);
        Task<List<Meta>> GetByUsuarioAndStatusAsync(Guid usuarioId, StatusMeta status);
        Task<List<Meta>> GetAtrasadasByUsuarioAsync(Guid usuarioId);
        Task<Meta?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task AddAsync(Meta meta);
        Task UpdateAsync(Meta meta);
        Task DeleteAsync(Meta meta);
        Task<int> GetTotalMetasByUsuarioAsync(Guid usuarioId);
        Task<int> GetMetasConcluidasByUsuarioAsync(Guid usuarioId);
        Task<List<Meta>> GetMetasProximasDoPrazoAsync(int dias = 7);
    }

    public class MetaRepository : BaseRepository<Meta>, IMetaRepository
    {
        public MetaRepository(MetaFlowDbContext context) : base(context) { }

        public async Task<(List<Meta> Metas, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = _dbSet
                .Include(m => m.Usuario)
                .AsNoTracking();

            query = ApplyOrdering(query, m => m.CriadoEm, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .AsNoTracking();

            query = ApplyOrdering(query, m => m.Prazo);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioAndStatusPagedAsync(Guid usuarioId, StatusMeta status, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(m => m.UsuarioId == usuarioId && m.Status == status.ToString())
                .AsNoTracking();

            query = ApplyOrdering(query, m => m.Prazo);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<Meta> Metas, int TotalCount)> GetByUsuarioAndCategoriaPagedAsync(Guid usuarioId, CategoriaMeta categoria, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(m => m.UsuarioId == usuarioId && m.Categoria == categoria.ToString())
                .AsNoTracking();

            query = ApplyOrdering(query, m => m.Prazo);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<List<Meta>> GetByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Meta>> GetByUsuarioAndStatusAsync(Guid usuarioId, StatusMeta status)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId && m.Status == status.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Meta>> GetAtrasadasByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId && 
                           m.Prazo < DateTime.Now && 
                           m.Status != StatusMeta.Concluida.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Meta?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
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

        public async Task<int> GetTotalMetasByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet.CountAsync(m => m.UsuarioId == usuarioId);
        }

        public async Task<int> GetMetasConcluidasByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet.CountAsync(m => 
                m.UsuarioId == usuarioId && 
                m.Status == StatusMeta.Concluida.ToString());
        }

        public async Task<List<Meta>> GetMetasProximasDoPrazoAsync(int dias = 7)
        {
            var dataLimite = DateTime.Now.AddDays(dias);
            return await _dbSet
                .Where(m => m.Prazo <= dataLimite && 
                           m.Prazo >= DateTime.Now &&
                           m.Status == StatusMeta.Ativa.ToString())
                .Include(m => m.Usuario)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}