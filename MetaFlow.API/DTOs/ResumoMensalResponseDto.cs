using Swashbuckle.AspNetCore.Annotations;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.DTOs
{
    [SwaggerSchema("DTO para resposta de resumo mensal")]
    public class ResumoMensalResponseDto
    {
        [SwaggerSchema("ID único do resumo")]
        public Guid Id { get; set; }

        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }

        [SwaggerSchema("Ano de referência")]
        public int Ano { get; set; }

        [SwaggerSchema("Mês de referência")]
        public int Mes { get; set; }

        [SwaggerSchema("Total de registros no mês")]
        public int TotalRegistros { get; set; }

        [SwaggerSchema("Metas concluídas no mês")]
        public int MetasConcluidas { get; set; }

        [SwaggerSchema("Média de humor do mês")]
        public decimal MediaHumor { get; set; }

        [SwaggerSchema("Média de produtividade do mês")]
        public decimal MediaProdutividade { get; set; }

        [SwaggerSchema("Taxa de conclusão de metas")]
        public decimal TaxaConclusao { get; set; }

        [SwaggerSchema("Data do cálculo")]
        public DateTime CalculadoEm { get; set; }

        [SwaggerSchema("Período formatado (MM/AAAA)")]
        public string PeriodoFormatado { get; set; } = string.Empty;

        [SwaggerSchema("Status de produtividade do mês")]
        public string StatusProdutividade { get; set; } = string.Empty;

        [SwaggerSchema("Status de humor do mês")]
        public string StatusHumor { get; set; } = string.Empty;

        [SwaggerSchema("Links HATEOAS para operações relacionadas")]
        public List<Link> Links { get; set; } = new List<Link>();
    }
}