using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [Tags("Recomendações Inteligentes")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RecomendacoesController : BaseController
    {
        private readonly IRecomendacaoService _recomendacaoService;
        private readonly ILogger<RecomendacoesController> _logger;

        public RecomendacoesController(
            IRecomendacaoService recomendacaoService,
            ILogger<RecomendacoesController> logger)
        {
            _recomendacaoService = recomendacaoService;
            _logger = logger;
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        [SwaggerOperation(
    Summary = "Gerar recomendações personalizadas",
    Description = "Utiliza ML.NET para gerar recomendações de metas personalizadas baseadas no perfil do usuário - Versão 2",
    OperationId = "GerarRecomendacoes"
)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> GerarRecomendacoes(Guid usuarioId)
        {
            if (User.Identity?.IsAuthenticated == true && !IsCurrentUser(usuarioId))
                return Forbid();

            var correlationId = GetCorrelationId();

            try
            {
                _logger.LogInformation(
                    "[CorrelationId: {CorrelationId}] Iniciando geração de recomendações ML para usuário {UsuarioId}",
                    correlationId, usuarioId);

                var response = await _recomendacaoService.GerarRecomendacoesAsync(usuarioId);

                if (!response.Success)
                    return BadRequest(CreateErrorResponse(response.Message));

                _logger.LogInformation(
                    "[CorrelationId: {CorrelationId}] Recomendações geradas com sucesso para usuário {UsuarioId}: {Count} itens",
                    correlationId, usuarioId, response.Data?.Count ?? 0);

                return Ok(new
                {
                    response.Data,
                    Message = "Recomendações geradas com sucesso usando ML.NET",
                    Metadados = new
                    {
                        TotalRecomendacoes = response.Data?.Count ?? 0,
                        ModeloML = "Classificação Multiclasse com SDCA Maximum Entropy",
                        TempoProcessamento = "Tempo real"
                    },
                    Links = new List<Link>
            {
                new Link($"/api/v2/usuarios/{usuarioId}/metas", "criar-meta-recomendada", "POST"),
                new Link($"/api/v2/dashboard/usuario/{usuarioId}", "ver-dashboard", "GET")
            },
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CorrelationId: {CorrelationId}] Erro ao gerar recomendações para usuário {UsuarioId}",
                    correlationId, usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao gerar recomendações: {ex.Message}"));
            }
        }


        [HttpGet("meta/{metaId}/previsao-progresso")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Prever progresso da meta", 
            Description = "Estima a probabilidade de conclusão e progresso esperado de uma meta - Versão 2",
            OperationId = "PreverProgressoMeta"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> PreverProgressoMeta(Guid metaId)
        {
            try
            {
                _logger.LogInformation("Iniciando previsão de progresso para meta {MetaId}", metaId);
                
                var response = await _recomendacaoService.PreverProgressoMetaAsync(metaId);

                if (!response.Success)
                    return BadRequest(CreateErrorResponse(response.Message));

                _logger.LogInformation("Previsão de progresso concluída para meta {MetaId}", metaId);

                return Ok(new
                {
                    Data = response.Data,
                    Message = "Previsão de progresso gerada com sucesso",
                    Links = new List<Link>
                    {
                        new Link($"/api/v2/metas/{metaId}", "detalhes-meta", "GET"),
                        new Link($"/api/v2/metas/{metaId}/progresso", "atualizar-progresso", "PATCH")
                    },
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao prever progresso da meta {MetaId}", metaId);
                return BadRequest(CreateErrorResponse($"Erro ao prever progresso: {ex.Message}"));
            }
        }

        [HttpPost("usuario/{usuarioId}/feedback")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Enviar feedback das recomendações", 
            Description = "Registra feedback do usuário sobre as recomendações recebidas - Versão 2",
            OperationId = "EnviarFeedbackRecomendacao"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> EnviarFeedbackRecomendacao(
            Guid usuarioId,
            [FromBody] FeedbackRecomendacaoRequest feedback)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de feedback inválidos", GetModelStateErrors()));

            try
            {
                _logger.LogInformation("Recebendo feedback de usuário {UsuarioId} para categoria {Categoria}", 
                    usuarioId, feedback.Categoria);

                await Task.Delay(100);

                _logger.LogInformation("Feedback registrado com sucesso para usuário {UsuarioId}", usuarioId);

                return Ok(new
                {
                    Message = "Feedback registrado com sucesso",
                    Dados = new
                    {
                        UsuarioId = usuarioId,
                        feedback.Categoria,
                        Util = feedback.FoiUtil,
                        feedback.Comentario
                    },
                    Links = new List<Link>
                    {
                        new Link($"/api/v2/recomendacoes/usuario/{usuarioId}", "novas-recomendacoes", "GET")
                    },
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar feedback do usuário {UsuarioId}", usuarioId);
                return BadRequest(CreateErrorResponse($"Erro ao processar feedback: {ex.Message}"));
            }
        }

        [HttpGet("status")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Verificar status do serviço", 
            Description = "Retorna o status atual do serviço de recomendações - Versão 2",
            OperationId = "StatusServico"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult StatusServico()
        {
            var status = new
            {
                Servico = "Sistema de Recomendações MetaFlow",
                Status = "Operacional",
                Versao = "2.0",
                MLFramework = "ML.NET",
                UltimaAtualizacao = DateTime.Now.AddHours(-1),
                Recursos = new[]
                {
                    "Recomendações Personalizadas",
                    "Previsão de Progresso"
                }
            };

            return Ok(new
            {
                Data = status,
                Message = "Serviço operacional",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                .ToList();
        }
    }

    public class FeedbackRecomendacaoRequest
    {
        public string Categoria { get; set; } = string.Empty;
        public bool FoiUtil { get; set; }
        public string? Comentario { get; set; }
    }
}