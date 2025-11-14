using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Registros Diários")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RegistroDiarioController : BaseController
    {
        private readonly IRegistroDiarioService _registroService;

        public RegistroDiarioController(IRegistroDiarioService registroService)
        {
            _registroService = registroService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar todos os registros", Description = "Retorna todos os registros diários cadastrados no sistema")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RegistroDiario>>> GetRegistros()
        {
            var response = await _registroService.GetRegistrosPagedAsync(new PaginationParams { PageNumber = 1, PageSize = 100 });
            return HandleServiceResponse(response);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar registros paginados", Description = "Retorna registros com paginação - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponse<RegistroDiario>>> GetRegistrosPaged([FromQuery] PaginationParams paginationParams)
        {
            var response = await _registroService.GetRegistrosPagedAsync(paginationParams);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar registros do usuário", Description = "Retorna todos os registros diários de um usuário específico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RegistroDiario>>> GetRegistrosByUsuario(Guid usuarioId)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _registroService.GetRegistrosByUsuarioAsync(usuarioId);
            return HandleServiceResponse(response);
        }

        [HttpGet("usuario/{usuarioId}/paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar registros do usuário paginados", Description = "Retorna registros do usuário com paginação - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponse<RegistroDiario>>> GetRegistrosByUsuarioPaged(
            Guid usuarioId, 
            [FromQuery] PaginationParams paginationParams)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _registroService.GetRegistrosByUsuarioPagedAsync(usuarioId, paginationParams);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obter registro específico", Description = "Retorna os detalhes de um registro diário específico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegistroDiario))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RegistroDiario>> GetRegistroById(Guid id)
        {
            var response = await _registroService.GetRegistroByIdAsync(id);
            return HandleServiceResponse(response);
        }


        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Criar novo registro", Description = "Cria um novo registro diário no sistema")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegistroDiario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<RegistroDiario>> CreateRegistro([FromBody] RegistroDiarioRequestDto registroDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados do registro inválidos"));

            var usuarioId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var response = await _registroService.CreateRegistroAsync(usuarioId, registroDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetRegistroById),
                new
                {
                    id = response.Data!.Id,
                },
                response.Data,
                "Registro diário criado com sucesso"
            );
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Atualizar registro", Description = "Atualiza os dados de um registro diário existente")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegistroDiario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<RegistroDiario>> UpdateRegistro(Guid id, [FromBody] RegistroDiarioRequestDto registroDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _registroService.UpdateRegistroAsync(id, registroDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Registro diário atualizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Excluir registro", Description = "Remove um registro diário do sistema")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteRegistro(Guid id)
        {
            var response = await _registroService.DeleteRegistroAsync(id);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        [HttpGet("usuario/{usuarioId}/estatisticas")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Obter estatísticas de registros", Description = "Retorna estatísticas dos registros do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> GetEstatisticasRegistros(Guid usuarioId)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _registroService.GetEstatisticasByUsuarioAsync(usuarioId);
            return HandleServiceResponse(response);
        }

        [HttpGet("usuario/{usuarioId}/ultimos/{quantidade}")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Obter últimos registros", Description = "Retorna os últimos registros do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RegistroDiario>>> GetUltimosRegistros(Guid usuarioId, int quantidade = 7)
        {
            if (User.IsInRole("Usuario") && !IsCurrentUser(usuarioId))
                return Forbid();

            var response = await _registroService.GetUltimosRegistrosAsync(usuarioId, quantidade);
            return HandleServiceResponse(response);
        }
    }
}