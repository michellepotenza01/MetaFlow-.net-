using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Tags("Metas")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MetaController : BaseController
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar metas do usuário")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMetasByUsuario(Guid usuarioId)
        {

            var response = await _metaService.GetMetasByUsuarioAsync(usuarioId);
            return HandleServiceResponse(response);
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar todas as metas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMetas()
        {
            var response = await _metaService.GetMetasPagedAsync(new PaginationParams { PageNumber = 1, PageSize = 100 });
            return HandleServiceResponse(response);
        }


        [HttpGet("paged")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar metas paginadas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMetasPaged([FromQuery] PaginationParams paginationParams)
        {
            var response = await _metaService.GetMetasPagedAsync(paginationParams);
            return HandleServiceResponse(response);
        }


        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Criar nova meta")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> CreateMeta([FromBody] MetaRequestDto metaDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var usuarioId))
                return Unauthorized(CreateErrorResponse("Usuário não autenticado"));

            var response = await _metaService.CreateMetaAsync(usuarioId, metaDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return CreatedAtAction(
                nameof(GetMetasByUsuario), 
                new { usuarioId, version = RequestedApiVersion }, 
                new {
                    response.Data,
                    Message = "Meta criada com sucesso",
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                }
            );
        }
            
                private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                .ToList();
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Atualizar meta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdateMeta(Guid id, [FromBody] MetaRequestDto metaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _metaService.UpdateMetaAsync(id, metaDto);
            return HandleServiceResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Excluir meta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteMeta(Guid id)
        {
            var response = await _metaService.DeleteMetaAsync(id);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        [HttpPost("{id}/concluir")]
        [Authorize]
        [SwaggerOperation(Summary = "Marcar meta como concluída")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ConcluirMeta(Guid id)
        {
            var progressoDto = new AtualizarProgressoRequestDto { Progresso = 100 };
            var response = await _metaService.UpdateProgressoAsync(id, progressoDto);
            return HandleServiceResponse(response);
        }
    }
}