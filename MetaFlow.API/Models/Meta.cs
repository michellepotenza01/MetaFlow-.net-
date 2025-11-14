using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using MetaFlow.API.Enums;
using MetaFlow.API.Converters;

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
        [SwaggerSchema("Categoria da meta")]
        public CategoriaMeta Categoria { get; set; }

        [Required(ErrorMessage = "O prazo é obrigatório.")]
        [SwaggerSchema("Data limite para conclusão")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime Prazo { get; set; }

        [Required(ErrorMessage = "O progresso é obrigatório.")]
        [Range(0, 100, ErrorMessage = "O progresso deve estar entre 0 e 100.")]
        [SwaggerSchema("Progresso atual (0-100%)")]
        public int Progresso { get; set; } = 0;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
        [SwaggerSchema("Descrição detalhada")]  
        public string? Descricao { get; set; }

        [SwaggerSchema("Data de criação")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "O status é obrigatório.")]
        [SwaggerSchema("Status da meta")]
        public StatusMeta Status { get; set; } = StatusMeta.Ativa;

        [NotMapped]
        [JsonIgnore]
        public string CategoriaString => Categoria.ToString();

        [NotMapped]
        [JsonIgnore]
        public string StatusString => Status.ToString();

        [ForeignKey("UsuarioId")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; } = null!;
        
        public void AtualizarStatusBaseadoNoProgresso()
        {
            if (Progresso >= 100)
                Status = StatusMeta.Concluida;
            else if (Status == StatusMeta.Concluida)
                Status = StatusMeta.Ativa;
        }

        public bool EstaAtrasada() => DateTime.Now > Prazo && Status != StatusMeta.Concluida;

        public int CalcularDiasRestantes() => Math.Max(0, (Prazo - DateTime.Now).Days);

        public bool PodeSerConcluida() => Status == StatusMeta.Ativa && Progresso >= 100;
    }
}