using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace MetaFlow.API.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("ID único do usuário")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        [SwaggerSchema("Nome completo do usuário")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(150, ErrorMessage = "O email deve ter no máximo 150 caracteres.")]
        [SwaggerSchema("Email único para login")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Hash da senha para segurança")]
        [JsonIgnore]
        public string SenhaHash { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "A profissão deve ter no máximo 100 caracteres.")]
        [SwaggerSchema("Cargo/posição atual")]
        public string? Profissao { get; set; }

        [StringLength(200, ErrorMessage = "O objetivo deve ter no máximo 200 caracteres.")]
        [SwaggerSchema("Objetivo profissional")]
        public string? ObjetivoProfissional { get; set; }

        [SwaggerSchema("Data de criação do registro")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [SwaggerSchema("Data da última atualização")]
        public DateTime AtualizadoEm { get; set; } = DateTime.Now;

        [JsonIgnore]
        public virtual ICollection<Meta> Metas { get; set; } = new List<Meta>();

        [JsonIgnore]
        public virtual ICollection<RegistroDiario> RegistrosDiarios { get; set; } = new List<RegistroDiario>();

        [JsonIgnore]
        public virtual ICollection<ResumoMensal> ResumosMensais { get; set; } = new List<ResumoMensal>();

        public void AtualizarDataModificacao()
        {
            AtualizadoEm = DateTime.Now;
        }
    }
}