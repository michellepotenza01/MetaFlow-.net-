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
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Usuario usuario);
        Task<int> GetTotalUsuariosAsync();
    }

    public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(MetaFlowDbContext context) : base(context) { }

        public async Task<(List<Usuario> Usuarios, int TotalCount)> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = _dbSet
                .Include(u => u.Metas)
                .Include(u => u.RegistrosDiarios)
                .AsNoTracking();

            query = ApplyOrdering(query, u => u.Nome);

            return await GetPagedAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Metas)
                .Include(u => u.RegistrosDiarios)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Metas)
                .Include(u => u.RegistrosDiarios)
                .Include(u => u.ResumosMensais)
                .FirstOrDefaultAsync(u => u.Id == id);
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
    }
}