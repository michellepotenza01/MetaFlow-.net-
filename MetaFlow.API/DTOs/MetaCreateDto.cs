using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Enums;
using MetaFlow.API.Converters; 
using System.Text.Json.Serialization; 

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para criação de metas")]
    public class MetaCreateDto
    {
        [Required(ErrorMessage = "O título da meta é obrigatório.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 200 caracteres.")]
        [SwaggerSchema("Título da meta")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [SwaggerSchema("Categoria da meta")]
        public CategoriaMeta Categoria { get; set; }

        [Required(ErrorMessage = "O prazo é obrigatório.")]
        [SwaggerSchema("Data limite para conclusão (formato: YYYY-MM-DD ou YYYY-MM-DDTHH:MM:SS)")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime Prazo { get; set; }

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
        [SwaggerSchema("Descrição detalhada")]
        public string? Descricao { get; set; }
    }
}