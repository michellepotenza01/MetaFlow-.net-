using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;


namespace MetaFlow.API.Models.Auth
{
    [SwaggerSchema("Resposta de login bem-sucedido")]
    public class LoginResponse
    {
        [SwaggerSchema("Token JWT para autenticação")]
        public string Token { get; set; } = string.Empty;

        [SwaggerSchema("Tipo do token")]
        public string TokenType { get; set; } = "Bearer";

        [SwaggerSchema("Data de expiração do token")]
        public DateTime ExpiresAt { get; set; }

        [SwaggerSchema("ID do usuário autenticado")]
        public Guid UsuarioId { get; set; }

        [SwaggerSchema("Nome do usuário autenticado")]
        public string Nome { get; set; } = string.Empty;

        [SwaggerSchema("Email do usuário autenticado")]
        public string Email { get; set; } = string.Empty;

        [SwaggerSchema("Tempo de expiração em horas")]
        public int ExpiresInHours { get; set; } = 24;

        [SwaggerSchema("Mensagem de sucesso")]
        public string Message { get; set; } = "Login realizado com sucesso";
    }
}