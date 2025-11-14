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
    [Tags("Análises")]
    [Produces("application/json")]
    public class AnalisesController : BaseController
    {
        private readonly IRecomendacaoService _recomendacaoService;

        public AnalisesController(IRecomendacaoService recomendacaoService)
        {
            _recomendacaoService = recomendacaoService;
        }

        [HttpGet("usuario/{usuarioId}/padroes")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Analisar padrões de comportamento", 
            Description = "Identifica padrões nos dados do usuário"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AnalisarPadroes(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _recomendacaoService.AnalisarPadroesAsync(usuarioId);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Padrões analisados com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        [HttpPost("usuario/{usuarioId}/sugerir-metas")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Sugerir novas metas", 
            Description = "Sugere metas personalizadas baseadas no perfil"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> SugerirMetas(Guid usuarioId)
        {
            if (!IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _recomendacaoService.GerarRecomendacoesAsync(usuarioId);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Metas sugeridas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }
    }
}