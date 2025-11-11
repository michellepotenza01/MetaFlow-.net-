using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace MetaFlow.API.Models
{
    [Table("RegistrosDiarios")]
    public class RegistroDiario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("ID único do registro")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        [SwaggerSchema("Data do registro")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O nível de humor é obrigatório.")]
        [Range(1, 10, ErrorMessage = "O humor deve estar entre 1 e 10.")]
        [SwaggerSchema("Nível de humor")]
        public int Humor { get; set; }

        [Required(ErrorMessage = "O nível de produtividade é obrigatório.")]
        [Range(1, 10, ErrorMessage = "A produtividade deve estar entre 1 e 10.")]
        [SwaggerSchema("Nível de produtividade")]
        public int Produtividade { get; set; }

        [Range(0, 1440, ErrorMessage = "O tempo de foco deve estar entre 0 e 1440 minutos.")]
        [SwaggerSchema("Minutos em atividades produtivas")]
        public int TempoFoco { get; set; } = 0;

        [StringLength(500, ErrorMessage = "As anotações devem ter no máximo 500 caracteres.")]
        [SwaggerSchema("Anotações do dia")]
        public string? Anotacoes { get; set; }

        [SwaggerSchema("Data de criação")]
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("UsuarioId")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; } = null!;

        public bool EhDataValida()
        {
            return Data.Date <= DateTime.Now.Date;
        }
    }
}