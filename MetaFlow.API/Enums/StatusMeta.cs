using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [SwaggerSchema("Status possíveis para uma meta")]
    public enum StatusMeta
    {
        [Display(Name = "Ativa", Description = "Meta em andamento")]
        Ativa,

        [Display(Name = "Concluída", Description = "Meta finalizada com sucesso")]
        Concluida,

        [Display(Name = "Cancelada", Description = "Meta cancelada pelo usuário")]
        Cancelada
    }
}