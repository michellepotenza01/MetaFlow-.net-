using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para atualização de progresso de meta")]
    public class AtualizarProgressoRequestDto
    {
        [Required(ErrorMessage = "O progresso é obrigatório.")]
        [Range(0, 100, ErrorMessage = "O progresso deve estar entre 0 e 100.")]
        [SwaggerSchema("Novo valor de progresso (0-100%)")]
        public int Progresso { get; set; }
    }
}