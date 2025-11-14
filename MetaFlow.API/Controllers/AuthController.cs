using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaFlow.API.Models.Auth;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Autenticação")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Realizar login no sistema", Description = "Autentica usuários no sistema e retorna token JWT")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de login inválidos"));

            var response = await _authService.LoginAsync(loginRequest);

            if (!response.Success)
                return Unauthorized(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Login realizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        [HttpGet("validate")]
        [MapToApiVersion("1.0")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Validar token JWT", Description = "Verifica a validade do token JWT atual")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult ValidateToken()
        {
            var userInfo = new
            {
                UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Nome = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            };

            return Ok(new
            {
                Message = "Token válido",
                Data = userInfo,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        [HttpGet("me")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obter informações do usuário atual (V2)", Description = "Retorna informações detalhadas do usuário autenticado - Versão 2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetCurrentUserV2()
        {
            var userInfo = new
            {
                UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Nome = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }),
                SessionId = Guid.NewGuid().ToString(),
                IssuedAt = DateTime.Now
            };

            return Ok(new
            {
                Data = userInfo,
                Message = "Informações do usuário recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }
    }
}