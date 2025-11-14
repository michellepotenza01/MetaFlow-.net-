using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Converters;
using System.Text.Json.Serialization; 

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de registros diários")]
    public class RegistroDiarioRequestDto
    {
        [Required(ErrorMessage = "A data é obrigatória.")]
        [SwaggerSchema("Data do registro (formato: YYYY-MM-DD ou YYYY-MM-DDTHH:MM:SS)")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))] 
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O nível de humor é obrigatório.")]
        [Range(1, 10, ErrorMessage = "O humor deve estar entre 1 e 10.")]
        [SwaggerSchema("Nível de humor (1-10)")]
        public int Humor { get; set; }

        [Required(ErrorMessage = "O nível de produtividade é obrigatório.")]
        [Range(1, 10, ErrorMessage = "A produtividade deve estar entre 1 e 10.")]
        [SwaggerSchema("Nível de produtividade (1-10)")]
        public int Produtividade { get; set; }

        [Range(0, 1440, ErrorMessage = "O tempo de foco deve estar entre 0 e 1440 minutos.")]
        [SwaggerSchema("Minutos em atividades produtivas")]
        public int TempoFoco { get; set; } = 0;

        [StringLength(500, ErrorMessage = "As anotações devem ter no máximo 500 caracteres.")]
        [SwaggerSchema("Anotações do dia")]
        public string? Anotacoes { get; set; }
    }
}