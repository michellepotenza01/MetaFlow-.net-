using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Enums;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para resposta de meta")]
    public class MetaResponseDto
    {
        [SwaggerSchema("ID único da meta")]
        public Guid Id { get; set; }

        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [SwaggerSchema("Título da meta")]
        public string Titulo { get; set; } = string.Empty;

        [SwaggerSchema("Categoria da meta")]
        public CategoriaMeta Categoria { get; set; }

        [SwaggerSchema("Data limite para conclusão")]
        public DateTime Prazo { get; set; }

        [SwaggerSchema("Progresso atual (0-100%)")]
        public int Progresso { get; set; }

        [SwaggerSchema("Descrição detalhada")]
        public string? Descricao { get; set; }

        [SwaggerSchema("Data de criação")]
        public DateTime CriadoEm { get; set; }

        [SwaggerSchema("Status da meta")]
        public StatusMeta Status { get; set; }

        [SwaggerSchema("Dias restantes para conclusão")]
        public int DiasRestantes { get; set; }

        [SwaggerSchema("Indica se a meta está atrasada")]
        public bool EstaAtrasada { get; set; }

        [SwaggerSchema("Links HATEOAS para operações relacionadas")]
        public List<Link> Links { get; set; } = new List<Link>();

        [SwaggerSchema("Indica se a meta pode ser concluída")]
        public bool PodeSerConcluida { get; set; }
    }
}