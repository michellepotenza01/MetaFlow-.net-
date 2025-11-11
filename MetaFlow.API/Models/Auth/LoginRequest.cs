using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.Auth
{
    [SwaggerSchema("Requisição de login")]
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(150, ErrorMessage = "O email deve ter no máximo 150 caracteres.")]
        [SwaggerSchema("Email do usuário")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Senha do usuário")]
        public string Senha { get; set; } = string.Empty;
    }
}


