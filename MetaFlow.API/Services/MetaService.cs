using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;

namespace MetaFlow.API.Services
{
    public interface IMetaService
    {
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasPagedAsync(PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioAndStatusPagedAsync(Guid usuarioId, StatusMeta status, PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioAndCategoriaPagedAsync(Guid usuarioId, CategoriaMeta categoria, PaginationParams paginationParams);
        Task<ServiceResponse<List<Meta>>> GetMetasByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<Meta>> GetMetaByIdAsync(Guid id);
        Task<ServiceResponse<Meta>> CreateMetaAsync(Guid usuarioId, MetaRequestDto metaDto);
        Task<ServiceResponse<Meta>> UpdateMetaAsync(Guid id, MetaRequestDto metaDto);
        Task<ServiceResponse<Meta>> UpdateProgressoAsync(Guid id, AtualizarProgressoRequestDto progressoDto);
        Task<ServiceResponse<bool>> DeleteMetaAsync(Guid id);
        Task<ServiceResponse<List<Meta>>> GetMetasAtrasadasByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<List<Meta>>> GetMetasProximasDoPrazoAsync(int dias = 7);
        Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId);
    }

    public class MetaService : IMetaService
    {
        private readonly IMetaRepository _metaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public MetaService(IMetaRepository metaRepository, IUsuarioRepository usuarioRepository)
        {
            _metaRepository = metaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _metaRepository.GetAllPagedAsync(paginationParams);
                var pagedResponse = new PagedResponse<Meta>(result.Metas, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, "Metas recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<Meta>>.NotFound("Usuário");

                var result = await _metaRepository.GetByUsuarioPagedAsync(usuarioId, paginationParams);
                var pagedResponse = new PagedResponse<Meta>(result.Metas, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, "Metas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioAndStatusPagedAsync(Guid usuarioId, StatusMeta status, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<Meta>>.NotFound("Usuário");

                var result = await _metaRepository.GetByUsuarioAndStatusPagedAsync(usuarioId, status, paginationParams);
                var pagedResponse = new PagedResponse<Meta>(result.Metas, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, $"Metas {status} do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas por status: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Meta>>> GetMetasByUsuarioAndCategoriaPagedAsync(Guid usuarioId, CategoriaMeta categoria, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<Meta>>.NotFound("Usuário");

                var result = await _metaRepository.GetByUsuarioAndCategoriaPagedAsync(usuarioId, categoria, paginationParams);
                var pagedResponse = new PagedResponse<Meta>(result.Metas, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Meta>>.Ok(pagedResponse, $"Metas da categoria {categoria} recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Meta>>.Error($"Erro ao buscar metas por categoria: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Meta>>> GetMetasByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<Meta>>.NotFound("Usuário");

                var metas = await _metaRepository.GetByUsuarioAsync(usuarioId);
                return ServiceResponse<List<Meta>>.Ok(metas, "Metas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Meta>> GetMetaByIdAsync(Guid id)
        {
            try
            {
                var meta = await _metaRepository.GetByIdAsync(id);
                return meta is null 
                    ? ServiceResponse<Meta>.NotFound("Meta")
                    : ServiceResponse<Meta>.Ok(meta, "Meta encontrada com sucesso");
            }
            catch (Exception ex)
            {
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

                if (metaDto.Prazo <= DateTime.Now)
                    return ServiceResponse<Meta>.Error("O prazo deve ser uma data futura");

                var meta = new Meta
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    Titulo = metaDto.Titulo.Trim(),
                    Categoria = metaDto.Categoria.ToString(),
                    Prazo = metaDto.Prazo,
                    Progresso = metaDto.Progresso,
                    Descricao = metaDto.Descricao?.Trim(),
                    Status = metaDto.Status.ToString(),
                    CriadoEm = DateTime.Now
                };

                meta.AtualizarStatus();

                await _metaRepository.AddAsync(meta);
                return ServiceResponse<Meta>.Ok(meta, "Meta criada com sucesso");
            }
            catch (Exception ex)
            {
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

                if (metaDto.Prazo <= DateTime.Now)
                    return ServiceResponse<Meta>.Error("O prazo deve ser uma data futura");

                metaExistente.Titulo = metaDto.Titulo.Trim();
                metaExistente.Categoria = metaDto.Categoria.ToString();
                metaExistente.Prazo = metaDto.Prazo;
                metaExistente.Progresso = metaDto.Progresso;
                metaExistente.Descricao = metaDto.Descricao?.Trim();
                metaExistente.Status = metaDto.Status.ToString();

                metaExistente.AtualizarStatus();

                await _metaRepository.UpdateAsync(metaExistente);
                return ServiceResponse<Meta>.Ok(metaExistente, "Meta atualizada com sucesso");
            }
            catch (Exception ex)
            {
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
                meta.AtualizarStatus();

                await _metaRepository.UpdateAsync(meta);
                return ServiceResponse<Meta>.Ok(meta, "Progresso da meta atualizado com sucesso");
            }
            catch (Exception ex)
            {
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

                var metasAtrasadas = await _metaRepository.GetAtrasadasByUsuarioAsync(usuarioId);
                return ServiceResponse<List<Meta>>.Ok(metasAtrasadas, "Metas atrasadas recuperadas com sucesso");
            }
            catch (Exception ex)
            {
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
                return ServiceResponse<List<Meta>>.Error($"Erro ao buscar metas próximas do prazo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var totalMetas = await _metaRepository.GetTotalMetasByUsuarioAsync(usuarioId);
                var metasConcluidas = await _metaRepository.GetMetasConcluidasByUsuarioAsync(usuarioId);
                var metasAtrasadas = (await _metaRepository.GetAtrasadasByUsuarioAsync(usuarioId)).Count;

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
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }
    }
}