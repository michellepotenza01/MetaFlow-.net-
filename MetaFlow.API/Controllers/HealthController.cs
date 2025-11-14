using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Health Check")]
    [Produces("application/json")]
    public class HealthController : BaseController
    {
        private readonly IHealthService _healthService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthService healthService, ILogger<HealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Verificar saúde completa da API", 
            Description = "Retorna status detalhado de todos os componentes do sistema"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetHealthComplete()
        {
            _logger.LogInformation("Health check completo solicitado - Versão 1.0");
            
            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success)
            {
                return StatusCode(503, new
                {
                    Status = "ServiceUnavailable",
                    Message = "Serviço indisponível",
                    Details = response.Data,
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                });
            }

            var healthData = response.Data!;
            
            var statusCode = healthData.Status switch
            {
                CustomHealthStatus.Healthy => StatusCodes.Status200OK,
                CustomHealthStatus.Degraded => StatusCodes.Status200OK,
                CustomHealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            return StatusCode(statusCode, new
            {
                Status = healthData.Status.ToString(),
                Message = "Health check realizado com sucesso",
                Data = healthData,
                Timestamp = healthData.Timestamp,
                Version = RequestedApiVersion
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(
            Summary = "Health Check Avançado (V2)", 
            Description = "Health check com métricas detalhadas e análise de performance - Versão 2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetHealthAdvanced()
        {
            _logger.LogInformation("Health check avançado solicitado - Versão 2.0");

            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success)
            {
                return StatusCode(503, new
                {
                    Status = "ServiceUnavailable",
                    Message = "Serviço indisponível",
                    Error = response.Message,
                    Timestamp = DateTime.Now,
                    Version = "2.0",
                    Severity = "CRITICAL"
                });
            }

            var healthData = response.Data!;

            var statusInfo = new
            {
                OverallStatus = healthData.Status.ToString(),
                healthData.Environment,
                Uptime = Environment.TickCount / 1000,
                healthData.Timestamp,
                ChecksPerformed = healthData.Entries.Count,
                TotalDuration = $"{healthData.TotalDuration.TotalMilliseconds}ms"
            };

            var detailedMetrics = healthData.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                    Metrics = e.Value.Data
                }
            );

            var statusCode = healthData.Status switch
            {
                CustomHealthStatus.Healthy => StatusCodes.Status200OK,
                CustomHealthStatus.Degraded => StatusCodes.Status200OK,
                CustomHealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            return StatusCode(statusCode, new
            {
                Status = statusInfo,
                Metrics = detailedMetrics,
                Message = healthData.Status == CustomHealthStatus.Healthy ? 
                         "Sistema operando normalmente" : 
                         "Sistema com problemas de saúde",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        [HttpGet("database")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Verificar saúde do banco de dados", 
            Description = "Retorna status detalhado da conexão com o banco de dados Oracle"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetDatabaseHealth()
        {
            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success || response.Data?.Entries == null)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Não foi possível verificar o banco de dados",
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                });
            }

            if (!response.Data.Entries.TryGetValue("database", out var databaseEntry))
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Informações do banco de dados não disponíveis",
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                });
            }

            var statusCode = databaseEntry.Status switch
            {
                CustomHealthStatus.Healthy => StatusCodes.Status200OK,
                CustomHealthStatus.Degraded => StatusCodes.Status200OK,
                CustomHealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            return StatusCode(statusCode, new
            {
                Status = databaseEntry.Status.ToString(),
                Message = databaseEntry.Description,
                Metrics = databaseEntry.Data,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }
    }
}