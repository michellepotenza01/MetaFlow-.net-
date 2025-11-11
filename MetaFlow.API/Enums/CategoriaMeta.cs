using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [SwaggerSchema("Categorias de metas disponíveis no sistema")]
    public enum CategoriaMeta
    {
        [Display(Name = "Carreira", Description = "Metas relacionadas à vida profissional")]
        Carreira,

        [Display(Name = "Saúde", Description = "Metas relacionadas à saúde e bem-estar")]
        Saude,

        [Display(Name = "Pessoal", Description = "Metas de desenvolvimento pessoal")]
        Pessoal,

        [Display(Name = "Educação", Description = "Metas de aprendizado e educação")]
        Educacao,

        [Display(Name = "Financeiro", Description = "Metas financeiras e investimentos")]
        Financeiro,

        [Display(Name = "Relacionamentos", Description = "Metas de vida social e relacionamentos")]
        Relacionamentos,

        [Display(Name = "Lazer", Description = "Metas de hobbies e tempo livre")]
        Lazer
    }
}