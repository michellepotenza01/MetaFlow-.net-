using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;

namespace MetaFlow.API.Services
{
    public interface IResumoMensalService
    {
        Task<ServiceResponse<PagedResponse<ResumoMensal>>> GetResumosPagedAsync(PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<ResumoMensal>>> GetResumosByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<ServiceResponse<List<ResumoMensal>>> GetResumosByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<ResumoMensal>> GetResumoByIdAsync(Guid id);
        Task<ServiceResponse<ResumoMensal>> GetResumoByUsuarioAndPeriodoAsync(Guid usuarioId, int ano, int mes);
        Task<ServiceResponse<ResumoMensal>> CreateResumoAsync(Guid usuarioId, int ano, int mes);
        Task<ServiceResponse<bool>> DeleteResumoAsync(Guid id);
        Task<ServiceResponse<ResumoMensal>> GetUltimoResumoByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<object>> CalcularResumoMensalAsync(Guid usuarioId, int ano, int mes);
    }

    public class ResumoMensalService : IResumoMensalService
    {
        private readonly IResumoMensalRepository _resumoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly IRegistroDiarioRepository _registroRepository;
        private readonly ILogger<ResumoMensalService> _logger;

        public ResumoMensalService(
            IResumoMensalRepository resumoRepository,
            IUsuarioRepository usuarioRepository,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository,
            ILogger<ResumoMensalService> logger)
        {
            _resumoRepository = resumoRepository;
            _usuarioRepository = usuarioRepository;
            _metaRepository = metaRepository;
            _registroRepository = registroRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<PagedResponse<ResumoMensal>>> GetResumosPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _resumoRepository.GetAllPagedAsync(paginationParams);
                
                var pagedResponse = new PagedResponse<ResumoMensal>(
                    result.Resumos, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize, 
                    result.TotalCount, 
                    new List<Link>() 
                );
                return ServiceResponse<PagedResponse<ResumoMensal>>.Ok(pagedResponse, "Resumos mensais recuperados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumos paginados");
                return ServiceResponse<PagedResponse<ResumoMensal>>.Error($"Erro ao buscar resumos: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<ResumoMensal>>> GetResumosByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<ResumoMensal>>.NotFound("Usuário");

                var result = await _resumoRepository.GetByUsuarioPagedAsync(usuarioId, paginationParams);
                
                var pagedResponse = new PagedResponse<ResumoMensal>(
                    result.Resumos, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize, 
                    result.TotalCount, 
                    new List<Link>() 
                );
                return ServiceResponse<PagedResponse<ResumoMensal>>.Ok(pagedResponse, "Resumos do usuário recuperados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumos paginados do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<PagedResponse<ResumoMensal>>.Error($"Erro ao buscar resumos do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<ResumoMensal>>> GetResumosByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<ResumoMensal>>.NotFound("Usuário");

                var resumos = await _resumoRepository.GetByUsuarioAsync(usuarioId);
                
                return ServiceResponse<List<ResumoMensal>>.Ok(resumos, "Resumos do usuário recuperados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumos do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<ResumoMensal>>.Error($"Erro ao buscar resumos do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<ResumoMensal>> GetResumoByIdAsync(Guid id)
        {
            try
            {
                var resumo = await _resumoRepository.GetByIdAsync(id);
                if (resumo is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Resumo mensal");

                return ServiceResponse<ResumoMensal>.Ok(resumo, "Resumo encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumo {ResumoId}", id);
                return ServiceResponse<ResumoMensal>.Error($"Erro ao buscar resumo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<ResumoMensal>> GetResumoByUsuarioAndPeriodoAsync(Guid usuarioId, int ano, int mes)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Usuário");

                var resumo = await _resumoRepository.GetByUsuarioAndPeriodoAsync(usuarioId, ano, mes);
                if (resumo is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Resumo para este período");

                return ServiceResponse<ResumoMensal>.Ok(resumo, "Resumo do período encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumo do período {Mes}/{Ano} para usuário {UsuarioId}", mes, ano, usuarioId);
                return ServiceResponse<ResumoMensal>.Error($"Erro ao buscar resumo do período: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<ResumoMensal>> CreateResumoAsync(Guid usuarioId, int ano, int mes)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Usuário");

                var resumoExistente = await _resumoRepository.GetByUsuarioAndPeriodoAsync(usuarioId, ano, mes);
                if (resumoExistente is not null)
                    return ServiceResponse<ResumoMensal>.Conflict("Já existe um resumo para este período");

                var calculoResult = await CalcularResumoMensalAsync(usuarioId, ano, mes);
                if (!calculoResult.Success)
                    return ServiceResponse<ResumoMensal>.Error(calculoResult.Message);

                var dadosCalculados = calculoResult.Data as dynamic;
                if (dadosCalculados == null)
                    return ServiceResponse<ResumoMensal>.Error("Erro ao calcular dados do resumo");

                var resumo = new ResumoMensal
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    Ano = ano,
                    Mes = mes,
                    TotalRegistros = dadosCalculados.TotalRegistros,
                    MetasConcluidas = dadosCalculados.MetasConcluidas,
                    MediaHumor = dadosCalculados.MediaHumor,
                    MediaProdutividade = dadosCalculados.MediaProdutividade,
                    TaxaConclusao = dadosCalculados.TaxaConclusao,
                    CalculadoEm = DateTime.Now
                };

                await _resumoRepository.AddAsync(resumo);
        
                return ServiceResponse<ResumoMensal>.Ok(resumo, "Resumo mensal criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar resumo para usuário {UsuarioId} - {Mes}/{Ano}", usuarioId, mes, ano);
                return ServiceResponse<ResumoMensal>.Error($"Erro ao criar resumo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteResumoAsync(Guid id)
        {
            try
            {
                var resumo = await _resumoRepository.GetByIdAsync(id);
                if (resumo is null)
                    return ServiceResponse<bool>.NotFound("Resumo mensal");

                await _resumoRepository.DeleteAsync(resumo);
                
                _logger.LogInformation("Resumo excluído: {ResumoId}", id);
                
                return ServiceResponse<bool>.Ok(true, "Resumo mensal excluído com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir resumo {ResumoId}", id);
                return ServiceResponse<bool>.Error($"Erro ao excluir resumo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<ResumoMensal>> GetUltimoResumoByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Usuário");

                var resumo = await _resumoRepository.GetUltimoResumoByUsuarioAsync(usuarioId);
                if (resumo is null)
                    return ServiceResponse<ResumoMensal>.NotFound("Resumo mensal");

                return ServiceResponse<ResumoMensal>.Ok(resumo, "Último resumo recuperado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar último resumo do usuário {UsuarioId}", usuarioId);
                return ServiceResponse<ResumoMensal>.Error($"Erro ao buscar último resumo: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> CalcularResumoMensalAsync(Guid usuarioId, int ano, int mes)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var dataInicio = new DateTime(ano, mes, 1);
                var dataFim = dataInicio.AddMonths(1).AddDays(-1);

                var registros = await _registroRepository.GetByUsuarioAndPeriodoAsync(usuarioId, dataInicio, dataFim);
                var metas = await _metaRepository.GetByUsuarioAsync(usuarioId);

                var totalRegistros = registros.Count;
                var metasConcluidasNoMes = metas.Count(m => 
                    m.Status == StatusMeta.Concluida && 
                    m.CriadoEm.Month == mes && 
                    m.CriadoEm.Year == ano);

                var mediaHumor = totalRegistros > 0 ? Math.Round(registros.Average(r => (decimal)r.Humor), 2) : 0;
                var mediaProdutividade = totalRegistros > 0 ? Math.Round(registros.Average(r => (decimal)r.Produtividade), 2) : 0;
                
                var totalMetasNoMes = metas.Count(m => 
                    m.CriadoEm.Month == mes && 
                    m.CriadoEm.Year == ano);
                
                var taxaConclusao = totalMetasNoMes > 0 ? 
                    Math.Round((decimal)metasConcluidasNoMes / totalMetasNoMes * 100, 2) : 0;

                var resumoCalculado = new
                {
                    TotalRegistros = totalRegistros,
                    MetasConcluidas = metasConcluidasNoMes,
                    MediaHumor = mediaHumor,
                    MediaProdutividade = mediaProdutividade,
                    TaxaConclusao = taxaConclusao
                };

                return ServiceResponse<object>.Ok(resumoCalculado, "Resumo calculado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular resumo para usuário {UsuarioId} - {Mes}/{Ano}", usuarioId, mes, ano);
                return ServiceResponse<object>.Error($"Erro ao calcular resumo: {ex.Message}");
            }
        }
    }
}