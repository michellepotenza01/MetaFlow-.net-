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
    [Tags("Usuários")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IAuthService _authService;
        private readonly IMetaService _metaService;
        private readonly IRegistroDiarioService _registroService;
        private readonly IResumoMensalService _resumoService;

        public UsuarioController(
            IUsuarioService usuarioService,
            IAuthService authService,
            IMetaService metaService,
            IRegistroDiarioService registroService,
            IResumoMensalService resumoService)
        {
            _usuarioService = usuarioService;
            _authService = authService;
            _metaService = metaService;
            _registroService = registroService;
            _resumoService = resumoService;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Registrar novo usuário", Description = "Cria uma nova conta de usuário no sistema")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Usuario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Usuario>> Registrar([FromBody] UsuarioRequestDto usuarioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados do usuário inválidos"));

            var response = await _authService.RegistrarAsync(usuarioDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetUsuarioById),
                new { id = response.Data!.Id, version = RequestedApiVersion },
                response.Data,
                "Usuário criado com sucesso"
            );
        }

        [HttpGet]
        [AllowAnonymous] 
        [SwaggerOperation(Summary = "Listar todos os usuários", Description = "Retorna todos os usuários cadastrados no sistema")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Usuario>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            var response = await _usuarioService.GetUsuariosAsync();
            return HandleServiceResponse(response);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar usuários paginados", Description = "Retorna usuários com paginação")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<Usuario>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<Usuario>>> GetUsuariosPaged([FromQuery] PaginationParams paginationParams)
        {
            var response = await _usuarioService.GetUsuariosPagedAsync(paginationParams);
            return HandleServiceResponse(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obter usuário específico", Description = "Retorna os detalhes de um usuário específico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Usuario))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Usuario>> GetUsuarioById(Guid id)
        {
           
            var response = await _usuarioService.GetUsuarioByIdAsync(id);
            return HandleServiceResponse(response);
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Atualizar usuário", Description = "Atualiza os dados de um usuário existente")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Usuario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Usuario>> UpdateUsuario(Guid id, [FromBody] UsuarioRequestDto usuarioDto)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _usuarioService.UpdateUsuarioAsync(id, usuarioDto);
            return HandleServiceResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Excluir usuário", Description = "Remove um usuário do sistema")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            var response = await _usuarioService.DeleteUsuarioAsync(id);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        [HttpGet("{id}/metas")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar metas do usuário", Description = "Retorna todas as metas de um usuário específico - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Meta>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<Meta>>> GetMetasByUsuario(Guid id)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            var response = await _metaService.GetMetasByUsuarioAsync(id);
            return HandleServiceResponse(response);
        }

        [HttpGet("{id}/registros")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar registros do usuário", Description = "Retorna todos os registros diários de um usuário específico - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RegistroDiario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RegistroDiario>>> GetRegistrosByUsuario(Guid id)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            var response = await _registroService.GetRegistrosByUsuarioAsync(id);
            return HandleServiceResponse(response);
        }

    

        [HttpGet("{id}/estatisticas")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Obter estatísticas do usuário", Description = "Retorna estatísticas detalhadas do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetEstatisticasUsuario(Guid id)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            var response = await _usuarioService.GetEstatisticasUsuarioAsync(id);
            return HandleServiceResponse(response);
        }

        [HttpPut("{id}/atualizar-senha")]
        [Authorize]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Atualizar senha do usuário", Description = "Atualiza a senha do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaRequest request)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de senha inválidos"));

            var response = await _usuarioService.AtualizarSenhaAsync(id, request);
            return HandleServiceResponse(response);
        }

        [HttpPut("{id}/atualizar-perfil")]
        [Authorize]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Atualizar perfil profissional", Description = "Atualiza informações profissionais do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AtualizarPerfil(Guid id, [FromBody] AtualizarPerfilRequest request)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de perfil inválidos"));

            var response = await _usuarioService.AtualizarPerfilAsync(id, request);
            return HandleServiceResponse(response);
        }

        [HttpDelete("{id}/desativar")]
        [Authorize]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Desativar conta", Description = "Desativa a conta do usuário - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DesativarConta(Guid id)
        {
            if (!IsCurrentUser(id))
                return Forbid();

            await Task.Delay(100);

            return Ok(new
            {
                Message = "Conta desativada com sucesso",
                UsuarioId = id,
                Timestamp = DateTime.UtcNow,
                Version = "2.0"
            });
        }
    }

   
}
