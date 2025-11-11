using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Repositories
{
    public interface IResumoMensalRepository
    {
        Task<(List<ResumoMensal> Resumos, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams);
        Task<(List<ResumoMensal> Resumos, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<List<ResumoMensal>> GetByUsuarioAsync(Guid usuarioId);
        Task<ResumoMensal?> GetByIdAsync(Guid id);
        Task<ResumoMensal?> GetByUsuarioAndPeriodoAsync(Guid usuarioId, int ano, int mes);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsResumoForPeriodoAsync(Guid usuarioId, int ano, int mes);
        Task AddAsync(ResumoMensal resumo);
        Task UpdateAsync(ResumoMensal resumo);
        Task DeleteAsync(ResumoMensal resumo);
        Task<ResumoMensal?> GetUltimoResumoByUsuarioAsync(Guid usuarioId);
        Task<List<ResumoMensal>> GetResumosByPeriodoAsync(int ano, int mes);
    }

    public class ResumoMensalRepository : BaseRepository<ResumoMensal>, IResumoMensalRepository
    {
        public ResumoMensalRepository(MetaFlowDbContext context) : base(context) { }

        public async Task<(List<ResumoMensal> Resumos, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = _dbSet
                .Include(rm => rm.Usuario)
                .AsNoTracking();

            query = ApplyOrdering(query, rm => rm.CalculadoEm, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<ResumoMensal> Resumos, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(rm => rm.UsuarioId == usuarioId)
                .AsNoTracking();

            query = ApplyOrdering(query, rm => rm.CalculadoEm, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<List<ResumoMensal>> GetByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(rm => rm.UsuarioId == usuarioId)
                .OrderByDescending(rm => rm.CalculadoEm)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ResumoMensal?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(rm => rm.Usuario)
                .FirstOrDefaultAsync(rm => rm.Id == id);
        }

        public async Task<ResumoMensal?> GetByUsuarioAndPeriodoAsync(Guid usuarioId, int ano, int mes)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rm => 
                    rm.UsuarioId == usuarioId && 
                    rm.Ano == ano && 
                    rm.Mes == mes);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(rm => rm.Id == id);
        }

        public async Task<bool> ExistsResumoForPeriodoAsync(Guid usuarioId, int ano, int mes)
        {
            return await _dbSet.AnyAsync(rm => 
                rm.UsuarioId == usuarioId && 
                rm.Ano == ano && 
                rm.Mes == mes);
        }

        public async Task AddAsync(ResumoMensal resumo)
        {
            await _dbSet.AddAsync(resumo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ResumoMensal resumo)
        {
            _dbSet.Update(resumo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ResumoMensal resumo)
        {
            _dbSet.Remove(resumo);
            await _context.SaveChangesAsync();
        }

        public async Task<ResumoMensal?> GetUltimoResumoByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(rm => rm.UsuarioId == usuarioId)
                .OrderByDescending(rm => rm.CalculadoEm)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ResumoMensal>> GetResumosByPeriodoAsync(int ano, int mes)
        {
            return await _dbSet
                .Where(rm => rm.Ano == ano && rm.Mes == mes)
                .Include(rm => rm.Usuario)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}