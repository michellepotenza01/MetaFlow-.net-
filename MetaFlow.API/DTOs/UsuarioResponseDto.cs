using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para resposta de usuário")]
    public class UsuarioResponseDto
    {
        [SwaggerSchema("ID único do usuário")]
        public Guid Id { get; set; }

        [SwaggerSchema("Nome completo do usuário")]
        public string Nome { get; set; } = string.Empty;

        [SwaggerSchema("Email do usuário")]
        public string Email { get; set; } = string.Empty;

        [SwaggerSchema("Cargo/posição atual")]
        public string? Profissao { get; set; }

        [SwaggerSchema("Objetivo profissional")]
        public string? ObjetivoProfissional { get; set; }

        [SwaggerSchema("Data de criação do registro")]
        public DateTime CriadoEm { get; set; }

        [SwaggerSchema("Data da última atualização")]
        public DateTime AtualizadoEm { get; set; }

        [SwaggerSchema("Indica se tem perfil profissional completo")]
        public string TemPerfilCompleto { get; set; } = "Não";
    }
}