using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;

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
        Task<ServiceResponse<Usuario>> AtualizarSenhaAsync(Guid usuarioId, AtualizarSenhaRequest request);
        Task<ServiceResponse<Usuario>> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly IRegistroDiarioRepository _registroRepository;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository,
            ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _metaRepository = metaRepository;
            _registroRepository = registroRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<PagedResponse<Usuario>>> GetUsuariosPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _usuarioRepository.GetAllPagedAsync(paginationParams);

                var pagedResponse = new PagedResponse<Usuario>(
                    result.Usuarios, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize, 
                    result.TotalCount, 
                    new List<Link>() 
                );
                return ServiceResponse<PagedResponse<Usuario>>.Ok(pagedResponse, "Usuários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuários paginados");
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
                _logger.LogError(ex, "Erro ao buscar usuários");
                return ServiceResponse<List<Usuario>>.Error($"Erro ao buscar usuários: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> GetUsuarioByIdAsync(Guid id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id, true);
                if (usuario is null)
                    return ServiceResponse<Usuario>.NotFound("Usuário");

                return ServiceResponse<Usuario>.Ok(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário {UsuarioId}", id);
                return ServiceResponse<Usuario>.Error($"Erro ao buscar usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario is null)
                    return ServiceResponse<Usuario>.NotFound("Usuário");

                return ServiceResponse<Usuario>.Ok(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email {Email}", email);
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
                    CriadoEm = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow
                };

                await _usuarioRepository.AddAsync(usuario);
                
                _logger.LogInformation("Usuário criado: {UsuarioId} - {Email}", usuario.Id, usuario.Email);
                
                return ServiceResponse<Usuario>.Ok(usuario, "Usuário criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário com email {Email}", usuarioDto.Email);
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

                if (usuarioExistente.Email != usuarioDto.Email && 
                    await _usuarioRepository.EmailExistsForOtherUserAsync(usuarioDto.Email, id))
                    return ServiceResponse<Usuario>.Conflict("Email já cadastrado");

                usuarioExistente.Nome = usuarioDto.Nome.Trim();
                usuarioExistente.Email = usuarioDto.Email.Trim();
                usuarioExistente.Profissao = usuarioDto.Profissao?.Trim();
                usuarioExistente.ObjetivoProfissional = usuarioDto.ObjetivoProfissional?.Trim();
                usuarioExistente.AtualizadoEm = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuarioExistente);
                
                _logger.LogInformation("Usuário atualizado: {UsuarioId}", id);
                
                var response = ServiceResponse<Usuario>.Ok(usuarioExistente, "Usuário atualizado com sucesso");
                response.Links.Add(new Link($"/api/v2/usuarios/{id}", "self", "GET"));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {UsuarioId}", id);
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
                
                _logger.LogInformation("Usuário excluído: {UsuarioId}", id);
                
                var response = ServiceResponse<bool>.Ok(true, "Usuário excluído com sucesso");
                response.Links.Add(new Link("/api/v2/usuarios", "listar-usuarios", "GET"));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {UsuarioId}", id);
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

                var totalMetas = await _metaRepository.CountByUsuarioAsync(usuarioId);
                var metasConcluidas = await _metaRepository.CountByUsuarioAsync(usuarioId, m => m.Status == StatusMeta.Concluida);
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
                    TempoNoSistema = (DateTime.UtcNow - usuario.CriadoEm).Days,
                    PerfilCompleto = !string.IsNullOrEmpty(usuario.Profissao) && !string.IsNullOrEmpty(usuario.ObjetivoProfissional)
                };

                var response = ServiceResponse<object>.Ok(estatisticas, "Estatísticas do usuário recuperadas com sucesso");
                response.Links.AddRange(new[]
                {
                    new Link($"/api/v2/usuarios/{usuarioId}/metas", "metas-usuario", "GET"),
                    new Link($"/api/v2/usuarios/{usuarioId}/registros", "registros-usuario", "GET"),
                    new Link($"/api/v2/dashboard/usuario/{usuarioId}", "dashboard", "GET")
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> AtualizarSenhaAsync(Guid usuarioId, AtualizarSenhaRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<Usuario>.NotFound("Usuário");

                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha, workFactor: 12);
                usuario.AtualizadoEm = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuario);
                
                _logger.LogInformation("Senha atualizada para usuário {UsuarioId}", usuarioId);
                
                var response = ServiceResponse<Usuario>.Ok(usuario, "Senha atualizada com sucesso");
                response.Links.Add(new Link($"/api/v2/usuarios/{usuarioId}", "self", "GET"));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar senha do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<Usuario>.Error($"Erro ao atualizar senha: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<Usuario>.NotFound("Usuário");

                usuario.Profissao = request.Profissao?.Trim();
                usuario.ObjetivoProfissional = request.ObjetivoProfissional?.Trim();
                usuario.AtualizadoEm = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuario);
                
                _logger.LogInformation("Perfil atualizado para usuário {UsuarioId}", usuarioId);
                
                var response = ServiceResponse<Usuario>.Ok(usuario, "Perfil atualizado com sucesso");
                response.Links.Add(new Link($"/api/v2/usuarios/{usuarioId}", "self", "GET"));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar perfil do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<Usuario>.Error($"Erro ao atualizar perfil: {ex.Message}");
            }
        }
    }

    public class AtualizarSenhaRequest
    {
        public string NovaSenha { get; set; } = string.Empty;
    }

    public class AtualizarPerfilRequest
    {
        public string? Profissao { get; set; }
        public string? ObjetivoProfissional { get; set; }
    }
}