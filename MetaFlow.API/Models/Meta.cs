using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace MetaFlow.API.Models
{
    [Table("Metas")]
    public class Meta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("ID único da meta")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "O título da meta é obrigatório.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 200 caracteres.")]
        [SwaggerSchema("Título da meta")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50, ErrorMessage = "A categoria deve ter no máximo 50 caracteres.")]
        [SwaggerSchema("Categoria da meta")]
        public string Categoria { get; set; } = string.Empty;

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

        [SwaggerSchema("Data de criação")]
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O status é obrigatório.")]
        [StringLength(20, ErrorMessage = "O status deve ter no máximo 20 caracteres.")]
        [SwaggerSchema("Status da meta")]
        public string Status { get; set; } = "Ativa";

        [ForeignKey("UsuarioId")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; } = null!;

        public void AtualizarStatus()
        {
            if (Progresso >= 100)
            {
                Status = "Concluída";
            }
        }

        public bool EstaAtrasada()
        {
            return DateTime.Now > Prazo && Status != "Concluída";
        }
    }
}