using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Dashboard")]
    [Produces("application/json")]
    public class DashboardController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IMetaService _metaService;
        private readonly IRegistroDiarioService _registroService;
        private readonly IResumoMensalService _resumoService;
        private readonly IRecomendacaoService _recomendacaoService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IUsuarioService usuarioService,
            IMetaService metaService,
            IRegistroDiarioService registroService,
            IResumoMensalService resumoService,
            IRecomendacaoService recomendacaoService,
            ILogger<DashboardController> logger)
        {
            _usuarioService = usuarioService;
            _metaService = metaService;
            _registroService = registroService;
            _resumoService = resumoService;
            _recomendacaoService = recomendacaoService;
            _logger = logger;
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        [SwaggerOperation(
     Summary = "Obter dashboard do usuário",
     Description = "Retorna dados consolidados para o dashboard pessoal do usuário"
 )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetDashboardUsuario(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            var correlationId = GetCorrelationId();
            _logger.LogInformation(
                "[CorrelationId: {CorrelationId}] Iniciando carregamento do dashboard para usuário {UsuarioId}",
                correlationId, usuarioId);

            try
            {
                var estatisticasUsuario = await _usuarioService.GetEstatisticasUsuarioAsync(usuarioId);
                var estatisticasMetas = await _metaService.GetEstatisticasByUsuarioAsync(usuarioId);
                var ultimosRegistros = await _registroService.GetUltimosRegistrosAsync(usuarioId, 5);
                var metasAtrasadas = await _metaService.GetMetasAtrasadasByUsuarioAsync(usuarioId);
                var metasRecentes = await _metaService.GetMetasRecentesAsync(usuarioId, 3);
                var recomendacoes = await _recomendacaoService.GerarRecomendacoesAsync(usuarioId);

                var dashboardData = new Dictionary<string, object>();

                dashboardData["EstatisticasUsuario"] = estatisticasUsuario.Success ?
                    estatisticasUsuario.Data! :
                    new { Mensagem = "Dados de usuário não disponíveis" };

                dashboardData["EstatisticasMetas"] = estatisticasMetas.Success ?
                    estatisticasMetas.Data! :
                    new { Mensagem = "Dados de metas não disponíveis" };

                dashboardData["UltimosRegistros"] = ultimosRegistros.Success ?
                    ultimosRegistros.Data! :
                    new List<object>();

                dashboardData["MetasAtrasadas"] = metasAtrasadas.Success ?
                    metasAtrasadas.Data! :
                    new List<object>();

                dashboardData["MetasRecentes"] = metasRecentes.Success ?
                    metasRecentes.Data! :
                    new List<object>();

                dashboardData["Recomendacoes"] = recomendacoes.Success ?
                    recomendacoes.Data! :
                    new List<object>();

                dashboardData["StatusGeral"] = new
                {
                    UsuarioDisponivel = estatisticasUsuario.Success,
                    MetasDisponiveis = estatisticasMetas.Success,
                    RegistrosDisponiveis = ultimosRegistros.Success,
                    MetasAtrasadasDisponiveis = metasAtrasadas.Success,
                    RecomendacoesDisponiveis = recomendacoes.Success
                };

                var response = new
                {
                    Data = dashboardData,
                    Message = "Dashboard recuperado com sucesso",
                    Links = new List<Link>
            {
                new Link($"/api/v{RequestedApiVersion}/usuarios/{usuarioId}/metas", "ver-metas", "GET"),
                new Link($"/api/v{RequestedApiVersion}/usuarios/{usuarioId}/registros", "ver-registros", "GET"),
                new Link($"/api/v{RequestedApiVersion}/recomendacoes/usuario/{usuarioId}", "ver-recomendacoes", "GET"),
                new Link($"/api/v{RequestedApiVersion}/registros", "criar-registro", "POST")
            },
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                };

                _logger.LogInformation(
                    "[CorrelationId: {CorrelationId}] Dashboard carregado com sucesso para usuário {UsuarioId}",
                    correlationId, usuarioId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CorrelationId: {CorrelationId}] Erro ao carregar dashboard para usuário {UsuarioId}",
                    correlationId, usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao carregar dashboard: {ex.Message}"));
            }
        }


        [HttpGet("usuario/{usuarioId}/resumo")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(
            Summary = "Obter resumo do dashboard (V2)", 
            Description = "Retorna resumo consolidado para o dashboard - Versão 2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetDashboardResumo(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            try
            {
                var estatisticasUsuario = await _usuarioService.GetEstatisticasUsuarioAsync(usuarioId);
                var estatisticasMetas = await _metaService.GetEstatisticasByUsuarioAsync(usuarioId);
                var ultimosRegistros = await _registroService.GetUltimosRegistrosAsync(usuarioId, 7);
                var ultimoResumo = await _resumoService.GetUltimoResumoByUsuarioAsync(usuarioId);

                object? usuarioData = estatisticasUsuario.Success ? estatisticasUsuario.Data : null;
                object? metasData = estatisticasMetas.Success ? estatisticasMetas.Data : null;
                object? ultimoResumoData = ultimoResumo.Success ? ultimoResumo.Data : null;

                var resumo = new
                {
                    Usuario = usuarioData,
                    Metas = metasData,
                    TotalRegistrosRecentes = ultimosRegistros.Success ? ultimosRegistros.Data?.Count ?? 0 : 0,
                    UltimoResumo = ultimoResumoData,
                    UltimaAtualizacao = DateTime.Now
                };

                var response = new
                {
                    Data = resumo,
                    Message = "Resumo do dashboard recuperado com sucesso",
                    Links = new List<Link>
                    {
                        new Link($"/api/v2/usuarios/{usuarioId}/metas", "ver-metas-completas", "GET"),
                        new Link($"/api/v2/usuarios/{usuarioId}/registros", "ver-registros-completos", "GET"),
                        new Link($"/api/v2/usuarios/{usuarioId}/resumos", "ver-resumos", "GET"),
                        new Link($"/api/v2/dashboard/usuario/{usuarioId}", "dashboard-completo", "GET")
                    },
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar resumo do dashboard para usuário {UsuarioId}", usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao carregar resumo do dashboard: {ex.Message}"));
            }
        }

        [HttpGet("usuario/{usuarioId}/quick-stats")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(
            Summary = "Obter estatísticas rápidas (V2)", 
            Description = "Retorna estatísticas rápidas para widgets do dashboard - Versão 2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetQuickStats(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            try
            {
                var totalMetas = await _metaService.GetEstatisticasByUsuarioAsync(usuarioId);
                var totalRegistros = await _registroService.GetEstatisticasByUsuarioAsync(usuarioId);
                var metasAtrasadas = await _metaService.GetMetasAtrasadasByUsuarioAsync(usuarioId);

                dynamic? metasData = totalMetas.Success ? totalMetas.Data : null;
                dynamic? registrosData = totalRegistros.Success ? totalRegistros.Data : null;

                var quickStats = new
                {
                    TotalMetas = metasData?.TotalMetas ?? 0,
                    MetasConcluidas = metasData?.MetasConcluidas ?? 0,
                    TotalRegistros = registrosData?.TotalRegistros ?? 0,
                    MetasAtrasadas = metasAtrasadas.Success ? metasAtrasadas.Data?.Count ?? 0 : 0,
                    TaxaConclusao = metasData?.TaxaConclusao ?? 0,
                    MediaHumor = registrosData?.MediaHumor ?? 0,
                    MediaProdutividade = registrosData?.MediaProdutividade ?? 0
                };

                return Ok(new
                {
                    Data = quickStats,
                    Message = "Estatísticas rápidas recuperadas com sucesso",
                    Links = new List<Link>
                    {
                        new Link($"/api/v2/dashboard/usuario/{usuarioId}", "dashboard-completo", "GET"),
                        new Link($"/api/v2/usuarios/{usuarioId}/metas", "gerenciar-metas", "GET")
                    },
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar estatísticas rápidas para usuário {UsuarioId}", usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao carregar estatísticas rápidas: {ex.Message}"));
            }
        }

        [HttpGet("usuario/{usuarioId}/alertas")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(
            Summary = "Obter alertas do dashboard (V2)", 
            Description = "Retorna alertas e notificações importantes para o usuário - Versão 2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> GetAlertas(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            try
            {
                var metasAtrasadas = await _metaService.GetMetasAtrasadasByUsuarioAsync(usuarioId);
                var ultimoRegistro = await _registroService.GetUltimosRegistrosAsync(usuarioId, 1);

                var alertas = new List<object>();

                if (metasAtrasadas.Success && metasAtrasadas.Data?.Count > 0)
                {
                    alertas.Add(new
                    {
                        Tipo = "atraso",
                        Titulo = "Metas Atrasadas",
                        Mensagem = $"Você tem {metasAtrasadas.Data.Count} meta(s) atrasada(s)",
                        Prioridade = "alta",
                        Acao = "revisar_metas"
                    });
                }

                if (ultimoRegistro.Success && ultimoRegistro.Data != null && 
                    (!ultimoRegistro.Data.Any() || 
                    (ultimoRegistro.Data.Any() && (DateTime.Now - ultimoRegistro.Data[0].Data).TotalDays > 3)))
                {
        
                    alertas.Add(new
                    {
                        Tipo = "registro",
                        Titulo = "Check-in Pendente",
                        Mensagem = "Faz tempo que você não faz um check-in diário",
                        Prioridade = "media",
                        Acao = "fazer_checkin"
                    });
                }

                var estatisticas = await _registroService.GetEstatisticasByUsuarioAsync(usuarioId);
                if (estatisticas.Success && estatisticas.Data != null)
                {
                    dynamic stats = estatisticas.Data;
                    if (stats.MediaProdutividade < 5)
                    {
                        alertas.Add(new
                        {
                            Tipo = "produtividade",
                            Titulo = "Produtividade Baixa",
                            Mensagem = "Sua produtividade está abaixo da média",
                            Prioridade = "baixa",
                            Acao = "ver_dicas"
                        });
                    }
                }

                return Ok(new
                {
                    Data = new { Alertas = alertas, Total = alertas.Count },
                    Message = alertas.Any() ? "Alertas recuperados com sucesso" : "Nenhum alerta no momento",
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar alertas para usuário {UsuarioId}", usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao carregar alertas: {ex.Message}"));
            }
        }
    }
}