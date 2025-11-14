using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Repositories
{
    public interface IRegistroDiarioRepository
    {
        Task<(List<RegistroDiario> Registros, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams);
        Task<(List<RegistroDiario> Registros, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<(List<RegistroDiario> Registros, int TotalCount)> GetByUsuarioAndPeriodoPagedAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim, PaginationParams paginationParams);
        Task<List<RegistroDiario>> GetByUsuarioAsync(Guid usuarioId);
        Task<List<RegistroDiario>> GetByUsuarioAndPeriodoAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim);
        Task<RegistroDiario?> GetByIdAsync(Guid id);
        Task<RegistroDiario?> GetByUsuarioAndDataAsync(Guid usuarioId, DateTime data);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsRegistroForDateAsync(Guid usuarioId, DateTime data);
        Task AddAsync(RegistroDiario registro);
        Task UpdateAsync(RegistroDiario registro);
        Task DeleteAsync(RegistroDiario registro);
        Task<int> GetTotalRegistrosByUsuarioAsync(Guid usuarioId);
        Task<decimal> GetMediaHumorByUsuarioAsync(Guid usuarioId);
        Task<decimal> GetMediaProdutividadeByUsuarioAsync(Guid usuarioId);
        Task<List<RegistroDiario>> GetUltimosRegistrosAsync(Guid usuarioId, int quantidade);
        
        Task<int> GetTotalRegistrosAsync();
        Task<decimal> GetMediaHumorGeralAsync();
        Task<decimal> GetMediaProdutividadeGeralAsync();
        Task<Dictionary<DayOfWeek, decimal>> GetProdutividadePorDiaDaSemanaAsync();
    }

    public class RegistroDiarioRepository : BaseRepository<RegistroDiario>, IRegistroDiarioRepository
    {
        public RegistroDiarioRepository(MetaFlowDbContext context) : base(context) { }

        public async Task<(List<RegistroDiario> Registros, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = _dbSet
                .Include(rd => rd.Usuario)
                .AsNoTracking();

            query = ApplyOrdering(query, rd => rd.Data, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<RegistroDiario> Registros, int TotalCount)> GetByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(rd => rd.UsuarioId == usuarioId)
                .AsNoTracking();

            query = ApplyOrdering(query, rd => rd.Data, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<(List<RegistroDiario> Registros, int TotalCount)> GetByUsuarioAndPeriodoPagedAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Where(rd => rd.UsuarioId == usuarioId && 
                            rd.Data >= dataInicio.Date && 
                            rd.Data <= dataFim.Date)
                .AsNoTracking();

            query = ApplyOrdering(query, rd => rd.Data, false);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<List<RegistroDiario>> GetByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(rd => rd.UsuarioId == usuarioId)
                .OrderByDescending(rd => rd.Data)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RegistroDiario>> GetByUsuarioAndPeriodoAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim)
        {
            return await _dbSet
                .Where(rd => rd.UsuarioId == usuarioId && 
                            rd.Data >= dataInicio.Date && 
                            rd.Data <= dataFim.Date)
                .OrderByDescending(rd => rd.Data)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RegistroDiario?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(rd => rd.Usuario)
                .FirstOrDefaultAsync(rd => rd.Id == id);
        }

        public async Task<RegistroDiario?> GetByUsuarioAndDataAsync(Guid usuarioId, DateTime data)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rd => 
                    rd.UsuarioId == usuarioId && 
                    rd.Data.Date == data.Date);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(rd => rd.Id == id);
        }

        public async Task<bool> ExistsRegistroForDateAsync(Guid usuarioId, DateTime data)
        {
            return await _dbSet.AnyAsync(rd => 
                rd.UsuarioId == usuarioId && 
                rd.Data.Date == data.Date);
        }

        public async Task AddAsync(RegistroDiario registro)
        {
            await _dbSet.AddAsync(registro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RegistroDiario registro)
        {
            _dbSet.Update(registro);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RegistroDiario registro)
        {
            _dbSet.Remove(registro);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalRegistrosByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet.CountAsync(rd => rd.UsuarioId == usuarioId);
        }

        public async Task<decimal> GetMediaHumorByUsuarioAsync(Guid usuarioId)
        {
            var media = await _dbSet
                .Where(rd => rd.UsuarioId == usuarioId)
                .AverageAsync(rd => (decimal?)rd.Humor) ?? 0;

            return Math.Round(media, 2);
        }

        public async Task<decimal> GetMediaProdutividadeByUsuarioAsync(Guid usuarioId)
        {
            var media = await _dbSet
                .Where(rd => rd.UsuarioId == usuarioId)
                .AverageAsync(rd => (decimal?)rd.Produtividade) ?? 0;

            return Math.Round(media, 2);
        }

        public async Task<List<RegistroDiario>> GetUltimosRegistrosAsync(Guid usuarioId, int quantidade)
        {
            return await _dbSet
                .Where(rd => rd.UsuarioId == usuarioId)
                .OrderByDescending(rd => rd.Data)
                .Take(quantidade)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalRegistrosAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<decimal> GetMediaHumorGeralAsync()
        {
            var media = await _dbSet
                .Where(r => r.Humor > 0)
                .AverageAsync(r => (decimal?)r.Humor) ?? 0;
            return Math.Round(media, 2);
        }

        public async Task<decimal> GetMediaProdutividadeGeralAsync()
        {
            var media = await _dbSet
                .Where(r => r.Produtividade > 0)
                .AverageAsync(r => (decimal?)r.Produtividade) ?? 0;
            return Math.Round(media, 2);
        }

        public async Task<Dictionary<DayOfWeek, decimal>> GetProdutividadePorDiaDaSemanaAsync()
        {
            var produtividadePorDia = await _dbSet
                .GroupBy(r => r.Data.DayOfWeek)
                .Select(g => new { Dia = g.Key, Media = g.Average(r => r.Produtividade) })
                .ToListAsync();

            return produtividadePorDia.ToDictionary(x => x.Dia, x => Math.Round((decimal)x.Media, 2));
        }
    }
}