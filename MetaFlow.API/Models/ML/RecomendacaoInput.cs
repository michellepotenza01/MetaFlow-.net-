using Microsoft.ML.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.ML
{
    [SwaggerSchema("Dados de entrada para sistema de recomendações")]
    public class RecomendacaoInput
    {
        [LoadColumn(0)]
        [SwaggerSchema("Categoria de meta mais frequente (codificada)")]
        public float CategoriaFrequenteEncoded { get; set; }

        [LoadColumn(1)]
        [SwaggerSchema("Taxa média de conclusão de metas")]
        public float TaxaConclusaoMedia { get; set; }

        [LoadColumn(2)]
        [SwaggerSchema("Duração média das metas (dias)")]
        public float DuracaoMediaMetas { get; set; }

        [LoadColumn(3)]
        [SwaggerSchema("Consistência de check-ins (%)")]
        public float ConsistenciaCheckins { get; set; }

        [LoadColumn(4)]
        [SwaggerSchema("Média de produtividade")]
        public float MediaProdutividade { get; set; }

        [LoadColumn(5)]
        [SwaggerSchema("Média de humor")]
        public float MediaHumor { get; set; }

        [LoadColumn(6)]
        [SwaggerSchema("Categoria recomendada (codificada)")]
        public float CategoriaRecomendada { get; set; }

        [NoColumn]
        [SwaggerSchema("ID do usuário")]
        public Guid UsuarioId { get; set; }
    }
}