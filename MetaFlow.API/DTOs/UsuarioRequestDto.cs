using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de usuários")]
    public class UsuarioRequestDto
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        [SwaggerSchema("Nome completo do usuário")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(150, ErrorMessage = "O email deve ter no máximo 150 caracteres.")]
        [SwaggerSchema("Email único para login")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Senha do usuário")]
        public string Senha { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "A profissão deve ter no máximo 100 caracteres.")]
        [SwaggerSchema("Cargo/posição atual")]
        public string? Profissao { get; set; }

        [StringLength(200, ErrorMessage = "O objetivo deve ter no máximo 200 caracteres.")]
        [SwaggerSchema("Objetivo profissional")]
        public string? ObjetivoProfissional { get; set; }
    }
}