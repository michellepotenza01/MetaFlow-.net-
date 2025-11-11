using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace MetaFlow.API.Models
{
    [Table("ResumosMensais")]
    public class ResumoMensal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("ID único do resumo")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "O ano é obrigatório.")]
        [Range(2020, 2100, ErrorMessage = "O ano deve estar entre 2020 e 2100.")]
        [SwaggerSchema("Ano de referência")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "O mês é obrigatório.")]
        [Range(1, 12, ErrorMessage = "O mês deve estar entre 1 e 12.")]
        [SwaggerSchema("Mês de referência")]
        public int Mes { get; set; }

        [Required]
        [Range(0, 31)]
        [SwaggerSchema("Total de registros no mês")]
        public int TotalRegistros { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        [SwaggerSchema("Metas concluídas no mês")]
        public int MetasConcluidas { get; set; } = 0;

        [Required]
        [Range(1, 10)]
        [SwaggerSchema("Média de humor do mês")]
        public decimal MediaHumor { get; set; } = 0;

        [Required]
        [Range(1, 10)]
        [SwaggerSchema("Média de produtividade do mês")]
        public decimal MediaProdutividade { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        [SwaggerSchema("Taxa de conclusão de metas")]
        public decimal TaxaConclusao { get; set; } = 0;

        [SwaggerSchema("Data do cálculo")]
        public DateTime CalculadoEm { get; set; } = DateTime.Now;

        [ForeignKey("UsuarioId")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}