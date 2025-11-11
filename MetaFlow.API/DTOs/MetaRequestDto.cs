using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Enums;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de metas")]
    public class MetaRequestDto
    {
        [Required(ErrorMessage = "O título da meta é obrigatório.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 200 caracteres.")]
        [SwaggerSchema("Título da meta")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [SwaggerSchema("Categoria da meta")]
        public CategoriaMeta Categoria { get; set; }

        [Required(ErrorMessage = "O prazo é obrigatório.")]
        [SwaggerSchema("Data limite para conclusão")]
        public DateTime Prazo { get; set; }

        [Required(ErrorMessage = "O progresso é obrigatório.")]
        [Range(0, 100, ErrorMessage = "O progresso deve estar entre 0 e 100.")]
        [SwaggerSchema("Progresso atual (0-100%)")]
        public int Progresso { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
        [SwaggerSchema("Descrição detalhada")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        [SwaggerSchema("Status da meta")]
        public StatusMeta Status { get; set; } = StatusMeta.Ativa;
    }
}