using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Models;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Repositories
{
    public interface IUsuarioRepository
    {
        Task<(List<Usuario> Usuarios, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams);
        Task<List<Usuario>> GetAllAsync();
        Task<(List<Usuario> Usuarios, int TotalCount)> GetPagedAsync(PaginationParams paginationParams);
        Task<Usuario?> GetByIdAsync(Guid id, bool includeRelacionamentos = false);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsForOtherUserAsync(string email, Guid usuarioId);
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Usuario usuario);
        Task<int> GetTotalUsuariosAsync();
        Task<Dictionary<string, int>> GetEstatisticasUsuariosAsync();
        
        Task<int> GetTotalUsuariosAtivosAsync();
        Task<int> GetNovosUsuariosUltimos7DiasAsync();
        Task<List<Usuario>> GetUsuariosMaisAtivosAsync(int quantidade = 10);
    }

    public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(MetaFlowDbContext context) : base(context) { }

        protected override IQueryable<Usuario> ApplyIncludes(IQueryable<Usuario> query)
        {
            return query
                .Include(u => u.Metas)
                .Include(u => u.RegistrosDiarios)
                .Include(u => u.ResumosMensais);
        }

        public async Task<(List<Usuario> Usuarios, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = ApplyIncludes(_dbSet);
            query = ApplyOrdering(query, u => u.Nome);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public new async Task<List<Usuario>> GetAllAsync()
        {
            return await ApplyIncludes(_dbSet)
                .OrderBy(u => u.Nome)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<Usuario> Usuarios, int TotalCount)> GetPagedAsync(PaginationParams paginationParams)
        {
            return await GetAllPagedAsync(paginationParams);
        }

        public async Task<Usuario?> GetByIdAsync(Guid id, bool includeRelacionamentos = false)
        {
            var query = _dbSet.AsQueryable();
            
            if (includeRelacionamentos)
                query = ApplyIncludes(query);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsForOtherUserAsync(string email, Guid usuarioId)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && u.Id != usuarioId);
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _dbSet.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _dbSet.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Usuario usuario)
        {
            _dbSet.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalUsuariosAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetEstatisticasUsuariosAsync()
        {
            var estatisticas = new Dictionary<string, int>
            {
                ["TotalUsuarios"] = await _dbSet.CountAsync(),
                ["UsuariosComPerfilCompleto"] = await _dbSet
                    .Where(u => !string.IsNullOrEmpty(u.Profissao) && !string.IsNullOrEmpty(u.ObjetivoProfissional))
                    .CountAsync(),
                ["UsuariosAtivos"] = await _dbSet
                    .Where(u => u.Metas.Any(m => m.Status == Enums.StatusMeta.Ativa))
                    .CountAsync()
            };

            return estatisticas;
        }

        public async Task<int> GetTotalUsuariosAtivosAsync()
        {
            var trintaDiasAtras = DateTime.UtcNow.AddDays(-30);
            return await _dbSet
                .CountAsync(u => u.Metas.Any(m => m.Status == Enums.StatusMeta.Ativa) ||
                               u.RegistrosDiarios.Any(r => r.Data >= trintaDiasAtras));
        }

        public async Task<int> GetNovosUsuariosUltimos7DiasAsync()
        {
            return await _dbSet
                .CountAsync(u => u.CriadoEm >= DateTime.UtcNow.AddDays(-7));
        }

        public async Task<List<Usuario>> GetUsuariosMaisAtivosAsync(int quantidade = 10)
        {
            return await _dbSet
                .OrderByDescending(u => u.RegistrosDiarios.Count)
                .Take(quantidade)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}