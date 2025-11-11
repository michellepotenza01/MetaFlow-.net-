using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Services
{
    public interface IUsuarioService
    {
        Task<ServiceResponse<PagedResponse<Usuario>>> GetUsuariosPagedAsync(PaginationParams paginationParams);
        Task<ServiceResponse<List<Usuario>>> GetUsuariosAsync();
        Task<ServiceResponse<Usuario>> GetUsuarioByIdAsync(Guid id);
        Task<ServiceResponse<Usuario>> GetUsuarioByEmailAsync(string email);
        Task<ServiceResponse<Usuario>> CreateUsuarioAsync(UsuarioRequestDto usuarioDto);
        Task<ServiceResponse<Usuario>> UpdateUsuarioAsync(Guid id, UsuarioRequestDto usuarioDto);
        Task<ServiceResponse<bool>> DeleteUsuarioAsync(Guid id);
        Task<ServiceResponse<object>> GetEstatisticasUsuarioAsync(Guid usuarioId);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly IRegistroDiarioRepository _registroRepository;
        private readonly IAuthService _authService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository,
            IAuthService authService)
        {
            _usuarioRepository = usuarioRepository;
            _metaRepository = metaRepository;
            _registroRepository = registroRepository;
            _authService = authService;
        }

        public async Task<ServiceResponse<PagedResponse<Usuario>>> GetUsuariosPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _usuarioRepository.GetAllPagedAsync(paginationParams);
                var pagedResponse = new PagedResponse<Usuario>(result.Usuarios, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Usuario>>.Ok(pagedResponse, "Usuários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Usuario>>.Error($"Erro ao buscar usuários: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Usuario>>> GetUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllAsync();
                return ServiceResponse<List<Usuario>>.Ok(usuarios, "Usuários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Usuario>>.Error($"Erro ao buscar usuários: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> GetUsuarioByIdAsync(Guid id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                return usuario is null 
                    ? ServiceResponse<Usuario>.NotFound("Usuário")
                    : ServiceResponse<Usuario>.Ok(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Usuario>.Error($"Erro ao buscar usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                return usuario is null 
                    ? ServiceResponse<Usuario>.NotFound("Usuário")
                    : ServiceResponse<Usuario>.Ok(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Usuario>.Error($"Erro ao buscar usuário por email: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> CreateUsuarioAsync(UsuarioRequestDto usuarioDto)
        {
            try
            {
                if (await _usuarioRepository.EmailExistsAsync(usuarioDto.Email))
                    return ServiceResponse<Usuario>.Conflict("Email já cadastrado");

                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = usuarioDto.Nome.Trim(),
                    Email = usuarioDto.Email.Trim(),
                    Profissao = usuarioDto.Profissao?.Trim(),
                    ObjetivoProfissional = usuarioDto.ObjetivoProfissional?.Trim(),
                    CriadoEm = DateTime.Now,
                    AtualizadoEm = DateTime.Now
                };

                var registroResult = await _authService.RegistrarAsync(usuario, usuarioDto.Senha);
                if (!registroResult.Success)
                    return ServiceResponse<Usuario>.Error(registroResult.Message);

                return ServiceResponse<Usuario>.Ok(usuario, "Usuário criado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Usuario>.Error($"Erro ao criar usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> UpdateUsuarioAsync(Guid id, UsuarioRequestDto usuarioDto)
        {
            try
            {
                var usuarioExistente = await _usuarioRepository.GetByIdAsync(id);
                if (usuarioExistente is null)
                    return ServiceResponse<Usuario>.NotFound("Usuário");

                if (usuarioExistente.Email != usuarioDto.Email && await _usuarioRepository.EmailExistsAsync(usuarioDto.Email))
                    return ServiceResponse<Usuario>.Conflict("Email já cadastrado");

                usuarioExistente.Nome = usuarioDto.Nome.Trim();
                usuarioExistente.Email = usuarioDto.Email.Trim();
                usuarioExistente.Profissao = usuarioDto.Profissao?.Trim();
                usuarioExistente.ObjetivoProfissional = usuarioDto.ObjetivoProfissional?.Trim();
                usuarioExistente.AtualizadoEm = DateTime.Now;

                if (!string.IsNullOrEmpty(usuarioDto.Senha))
                    usuarioExistente.SenhaHash = _authService.HashPassword(usuarioDto.Senha);

                await _usuarioRepository.UpdateAsync(usuarioExistente);
                return ServiceResponse<Usuario>.Ok(usuarioExistente, "Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Usuario>.Error($"Erro ao atualizar usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteUsuarioAsync(Guid id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario is null)
                    return ServiceResponse<bool>.NotFound("Usuário");

                var metas = await _metaRepository.GetByUsuarioAsync(id);
                var registros = await _registroRepository.GetByUsuarioAsync(id);

                if (metas.Any() || registros.Any())
                    return ServiceResponse<bool>.Error("Não é possível excluir usuário com metas ou registros associados");

                await _usuarioRepository.DeleteAsync(usuario);
                return ServiceResponse<bool>.Ok(true, "Usuário excluído com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao excluir usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> GetEstatisticasUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var totalMetas = await _metaRepository.GetTotalMetasByUsuarioAsync(usuarioId);
                var metasConcluidas = await _metaRepository.GetMetasConcluidasByUsuarioAsync(usuarioId);
                var totalRegistros = await _registroRepository.GetTotalRegistrosByUsuarioAsync(usuarioId);
                var mediaHumor = await _registroRepository.GetMediaHumorByUsuarioAsync(usuarioId);
                var mediaProdutividade = await _registroRepository.GetMediaProdutividadeByUsuarioAsync(usuarioId);

                var estatisticas = new
                {
                    TotalMetas = totalMetas,
                    MetasConcluidas = metasConcluidas,
                    TotalRegistros = totalRegistros,
                    MediaHumor = Math.Round(mediaHumor, 2),
                    MediaProdutividade = Math.Round(mediaProdutividade, 2),
                    TaxaConclusao = totalMetas > 0 ? Math.Round((decimal)metasConcluidas / totalMetas * 100, 2) : 0,
                    TempoNoSistema = (DateTime.Now - usuario.CriadoEm).Days
                };

                return ServiceResponse<object>.Ok(estatisticas, "Estatísticas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }
    }
}