using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;
using System.Linq.Expressions;

namespace MetaFlow.API.Services
{
    public interface IMetaService
    {
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasPagedAsync(PaginationParams paginationParams, Expression<Func<Meta, bool>>? filter = null);
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams, Expression<Func<Meta, bool>>? filter = null);
        Task<ServiceResponse<List<Meta>>> GetMetasByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null);
        Task<ServiceResponse<Meta>> GetMetaByIdAsync(Guid id);
        Task<ServiceResponse<Meta>> GetMetaByUsuarioAndIdAsync(Guid usuarioId, Guid metaId);
        Task<ServiceResponse<Meta>> CreateMetaAsync(Guid usuarioId, MetaRequestDto metaDto);
        Task<ServiceResponse<Meta>> UpdateMetaAsync(Guid id, MetaRequestDto metaDto);
        Task<ServiceResponse<Meta>> UpdateProgressoAsync(Guid id, AtualizarProgressoRequestDto progressoDto);
        Task<ServiceResponse<bool>> DeleteMetaAsync(Guid id);
        Task<ServiceResponse<List<Meta>>> GetMetasAtrasadasByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<List<Meta>>> GetMetasProximasDoPrazoAsync(int dias = 7);
        Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<Dictionary<StatusMeta, int>>> GetEstatisticasStatusByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<List<Meta>>> GetMetasRecentesAsync(Guid usuarioId, int quantidade = 5);
    }

    public class MetaService : IMetaService
    {
        private readonly IMetaRepository _metaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<MetaService> _logger;

        public MetaService(
            IMetaRepository metaRepository, 
            IUsuarioRepository usuarioRepository, 
            ILogger<MetaService> logger)
        {
            _metaRepository = metaRepository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasPagedAsync(
            PaginationParams paginationParams, 
            Expression<Func<Meta, bool>>? filter = null)
        {
            try
            {
                var result = await _metaRepository.GetPagedAsync(filter, paginationParams);

                var pagedResponse = new PagedResponse<Meta>(
                    result.Metas, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize, 
                    result.TotalCount, 
                    new List<Link>() 
                );
                
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, "Metas recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas paginadas");
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioPagedAsync(
            Guid usuarioId, 
            PaginationParams paginationParams, 
            Expression<Func<Meta, bool>>? filter = null)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<Meta>>.NotFound("Usuário");

                Expression<Func<Meta, bool>> usuarioFilter = m => m.UsuarioId == usuarioId;
                Expression<Func<Meta, bool>> finalFilter = filter != null 
                    ? Expression.Lambda<Func<Meta, bool>>(
                        Expression.AndAlso(usuarioFilter.Body, filter.Body), 
                        usuarioFilter.Parameters[0])
                    : usuarioFilter;

                var result = await _metaRepository.GetPagedAsync(finalFilter, paginationParams);
                
                var pagedResponse = new PagedResponse<Meta>(
                    result.Metas, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize, 
                    result.TotalCount, 
                    new List<Link>() 
                );
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, "Metas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas paginadas do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Meta>>> GetMetasByUsuarioAsync(Guid usuarioId, Expression<Func<Meta, bool>>? filter = null)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<Meta>>.NotFound("Usuário");

                var metas = await _metaRepository.GetByUsuarioAsync(usuarioId, filter);

                return ServiceResponse<List<Meta>>.Ok(metas, "Metas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> GetMetaByIdAsync(Guid id)
        {
            try
            {
                var meta = await _metaRepository.GetByIdAsync(id, true);
                if (meta is null)
                    return ServiceResponse<Meta>.NotFound("Meta");

                return ServiceResponse<Meta>.Ok(meta, "Meta encontrada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar meta {MetaId}", id);
                return ServiceResponse<Meta>.Error($"Erro ao buscar meta: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> GetMetaByUsuarioAndIdAsync(Guid usuarioId, Guid metaId)
        {
            try
            {
                var meta = await _metaRepository.GetByUsuarioAndIdAsync(usuarioId, metaId);
                if (meta is null)
                    return ServiceResponse<Meta>.NotFound("Meta");

                return ServiceResponse<Meta>.Ok(meta, "Meta encontrada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar meta {MetaId} do usuário {UsuarioId}", metaId, usuarioId);
                return ServiceResponse<Meta>.Error($"Erro ao buscar meta: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> CreateMetaAsync(Guid usuarioId, MetaRequestDto metaDto)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<Meta>.NotFound("Usuário");

                if (metaDto.Prazo.Date <= DateTime.Now.Date)
                    return ServiceResponse<Meta>.Error("O prazo deve ser uma data futura");

                if (metaDto.Prazo > DateTime.Now.AddYears(1))
                    return ServiceResponse<Meta>.Error("O prazo não pode ser superior a 1 ano");

                var meta = new Meta
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    Titulo = metaDto.Titulo.Trim(),
                    Categoria = metaDto.Categoria,
                    Prazo = metaDto.Prazo,
                    Progresso = metaDto.Progresso,
                    Descricao = metaDto.Descricao?.Trim(),
                    Status = metaDto.Status,
                    CriadoEm = DateTime.Now
                };

                meta.AtualizarStatusBaseadoNoProgresso();

                await _metaRepository.AddAsync(meta);
                
                return ServiceResponse<Meta>.Ok(meta, "Meta criada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar meta para usuário {UsuarioId}", usuarioId);
                return ServiceResponse<Meta>.Error($"Erro ao criar meta: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> UpdateMetaAsync(Guid id, MetaRequestDto metaDto)
        {
            try
            {
                var metaExistente = await _metaRepository.GetByIdAsync(id);
                if (metaExistente is null)
                    return ServiceResponse<Meta>.NotFound("Meta");

                if (metaDto.Prazo.Date <= DateTime.Now.Date)
                    return ServiceResponse<Meta>.Error("O prazo deve ser uma data futura");

                if (metaDto.Prazo > DateTime.Now.AddYears(1))
                    return ServiceResponse<Meta>.Error("O prazo não pode ser superior a 1 ano");

                metaExistente.Titulo = metaDto.Titulo.Trim();
                metaExistente.Categoria = metaDto.Categoria;
                metaExistente.Prazo = metaDto.Prazo;
                metaExistente.Progresso = metaDto.Progresso;
                metaExistente.Descricao = metaDto.Descricao?.Trim();
                metaExistente.Status = metaDto.Status;

                metaExistente.AtualizarStatusBaseadoNoProgresso();

                await _metaRepository.UpdateAsync(metaExistente);
                
                return ServiceResponse<Meta>.Ok(metaExistente, "Meta atualizada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar meta {MetaId}", id);
                return ServiceResponse<Meta>.Error($"Erro ao atualizar meta: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> UpdateProgressoAsync(Guid id, AtualizarProgressoRequestDto progressoDto)
        {
            try
            {
                var meta = await _metaRepository.GetByIdAsync(id);
                if (meta is null)
                    return ServiceResponse<Meta>.NotFound("Meta");

                meta.Progresso = progressoDto.Progresso;
                meta.AtualizarStatusBaseadoNoProgresso();

                await _metaRepository.UpdateAsync(meta);
                
                return ServiceResponse<Meta>.Ok(meta, "Progresso da meta atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar progresso da meta {MetaId}", id);
                return ServiceResponse<Meta>.Error($"Erro ao atualizar progresso: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteMetaAsync(Guid id)
        {
            try
            {
                var meta = await _metaRepository.GetByIdAsync(id);
                if (meta is null)
                    return ServiceResponse<bool>.NotFound("Meta");

                await _metaRepository.DeleteAsync(meta);
                
                return ServiceResponse<bool>.Ok(true, "Meta excluída com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir meta {MetaId}", id);
                return ServiceResponse<bool>.Error($"Erro ao excluir meta: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Meta>>> GetMetasAtrasadasByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<Meta>>.NotFound("Usuário");

                Expression<Func<Meta, bool>> filter = m => 
                    m.Prazo < DateTime.Now && 
                    m.Status != StatusMeta.Concluida;

                var metasAtrasadas = await _metaRepository.GetByUsuarioAsync(usuarioId, filter);
                
                return ServiceResponse<List<Meta>>.Ok(metasAtrasadas, "Metas atrasadas recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas atrasadas do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas atrasadas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Meta>>> GetMetasProximasDoPrazoAsync(int dias = 7)
        {
            try
            {
                var metas = await _metaRepository.GetMetasProximasDoPrazoAsync(dias);
                
                return ServiceResponse<List<Meta>>.Ok(metas, "Metas próximas do prazo recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas próximas do prazo");
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas próximas do prazo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Meta>>> GetMetasRecentesAsync(Guid usuarioId, int quantidade = 5)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<Meta>>.NotFound("Usuário");

                var metas = await _metaRepository.GetByUsuarioAsync(usuarioId);
                var metasRecentes = metas
                    .OrderByDescending(m => m.CriadoEm)
                    .Take(quantidade)
                    .ToList();
                
                return ServiceResponse<List<Meta>>.Ok(metasRecentes, "Metas recentes recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar metas recentes do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas recentes: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var totalMetas = await _metaRepository.CountByUsuarioAsync(usuarioId);
                var metasConcluidas = await _metaRepository.CountByUsuarioAsync(usuarioId, m => m.Status == StatusMeta.Concluida);
                var metasAtrasadas = await _metaRepository.CountByUsuarioAsync(usuarioId, m => 
                    m.Prazo < DateTime.Now && m.Status != StatusMeta.Concluida);

                var estatisticas = new
                {
                    TotalMetas = totalMetas,
                    MetasConcluidas = metasConcluidas,
                    MetasAtrasadas = metasAtrasadas,
                    MetasAtivas = totalMetas - metasConcluidas - metasAtrasadas,
                    TaxaConclusao = totalMetas > 0 ? Math.Round((decimal)metasConcluidas / totalMetas * 100, 2) : 0
                };

                return ServiceResponse<object>.Ok(estatisticas, "Estatísticas recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Dictionary<StatusMeta, int>>> GetEstatisticasStatusByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<Dictionary<StatusMeta, int>>.NotFound("Usuário");

                var estatisticas = await _metaRepository.GetEstatisticasStatusAsync(usuarioId);
                
                return ServiceResponse<Dictionary<StatusMeta, int>>.Ok(estatisticas, "Estatísticas por status recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas por status do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<Dictionary<StatusMeta, int>>.Error($"Erro ao buscar estatísticas por status: {ex.Message}");
            }
        }
    }
}