using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Models;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Resumos Mensais")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ResumoMensalController : BaseController
    {
        private readonly IResumoMensalService _resumoService;

        public ResumoMensalController(IResumoMensalService resumoService)
        {
            _resumoService = resumoService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar todos os resumos", Description = "Retorna todos os resumos mensais cadastrados no sistema")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ResumoMensal>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ResumoMensal>>> GetResumos()
        {
            var response = await _resumoService.GetResumosPagedAsync(new PaginationParams { PageNumber = 1, PageSize = 100 });
            return HandleServiceResponse(response);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar resumos paginados", Description = "Retorna resumos com paginação - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<ResumoMensal>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponse<ResumoMensal>>> GetResumosPaged([FromQuery] PaginationParams paginationParams)
        {
            var response = await _resumoService.GetResumosPagedAsync(paginationParams);
            if (!response.Success || response.Data == null)
        return HandleServiceResponse(response); 

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar resumos do usuário", Description = "Retorna todos os resumos mensais de um usuário específico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ResumoMensal>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ResumoMensal>>> GetResumosByUsuario(Guid usuarioId)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _resumoService.GetResumosByUsuarioAsync(usuarioId);
        return HandleServiceResponse(response); 
        }

        [HttpGet("usuario/{usuarioId}/paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar resumos do usuário paginados", Description = "Retorna resumos do usuário com paginação - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<ResumoMensal>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponse<ResumoMensal>>> GetResumosByUsuarioPaged(
            Guid usuarioId, 
            [FromQuery] PaginationParams paginationParams)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _resumoService.GetResumosByUsuarioPagedAsync(usuarioId, paginationParams);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obter resumo específico", Description = "Retorna os detalhes de um resumo mensal específico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResumoMensal))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResumoMensal>> GetResumoById(Guid id)
        {
            var response = await _resumoService.GetResumoByIdAsync(id);
            return HandleServiceResponse(response);
        }


        [HttpPost("usuario/{usuarioId}/periodo/{ano}/{mes}")]
        [Authorize]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Criar resumo mensal", Description = "Cria um novo resumo mensal para o usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResumoMensal))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<ResumoMensal>> CreateResumo(Guid usuarioId, int ano, int mes)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            if (mes < 1 || mes > 12)
                return BadRequest(CreateErrorResponse("Mês deve estar entre 1 e 12"));

            var response = await _resumoService.CreateResumoAsync(usuarioId, ano, mes);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetResumoById),
                new { id = response.Data!.Id, version = RequestedApiVersion },
                response.Data,
                "Resumo mensal criado com sucesso"
            );
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Excluir resumo", Description = "Remove um resumo mensal do sistema")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteResumo(Guid id)
        {
            var response = await _resumoService.DeleteResumoAsync(id);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        [HttpGet("usuario/{usuarioId}/ultimo")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Obter último resumo", Description = "Retorna o último resumo mensal do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResumoMensal))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResumoMensal>> GetUltimoResumo(Guid usuarioId)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _resumoService.GetUltimoResumoByUsuarioAsync(usuarioId);
            return HandleServiceResponse(response);
        }

    }
}