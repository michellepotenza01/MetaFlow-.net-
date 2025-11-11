using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para resposta de registro diário")]
    public class RegistroDiarioResponseDto
    {
        [SwaggerSchema("ID único do registro")]
        public Guid Id { get; set; }

        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [SwaggerSchema("Data do registro")]
        public DateTime Data { get; set; }

        [SwaggerSchema("Nível de humor")]
        public int Humor { get; set; }

        [SwaggerSchema("Nível de produtividade")]
        public int Produtividade { get; set; }

        [SwaggerSchema("Minutos em atividades produtivas")]
        public int TempoFoco { get; set; }

        [SwaggerSchema("Anotações do dia")]
        public string? Anotacoes { get; set; }

        [SwaggerSchema("Data de criação")]
        public DateTime CriadoEm { get; set; }

        [SwaggerSchema("Status de produtividade do dia")]
        public string StatusProdutividade { get; set; } = string.Empty;

        [SwaggerSchema("Status de humor do dia")]
        public string StatusHumor { get; set; } = string.Empty;

        [SwaggerSchema("Dia da semana")]
        public string DiaDaSemana { get; set; } = string.Empty;
    }
}