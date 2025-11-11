using Microsoft.ML.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.ML
{
    [SwaggerSchema("Resultado da recomendação de metas")]
    public class RecomendacaoPrediction
    {
        [ColumnName("PredictedLabel")]
        [SwaggerSchema("Categoria recomendada (codificada)")]
        public float CategoriaRecomendada { get; set; }

        [ColumnName("Score")]
        [SwaggerSchema("Score da predição")]
        public float[]? Score { get; set; }

        [SwaggerSchema("Categoria recomendada (texto)")]
        public string Categoria => CategoriaRecomendada switch
        {
            0 => "Carreira",
            1 => "Saúde",
            2 => "Pessoal",
            3 => "Educação",
            4 => "Financeiro",
            5 => "Relacionamentos",
            6 => "Lazer",
            _ => "Pessoal"
        };

        [SwaggerSchema("Confiança da recomendação")]
        public float Confianca => Score?[(int)CategoriaRecomendada] ?? 0.5f;

        [SwaggerSchema("Justificativa da recomendação")]
        public string Justificativa => GerarJustificativa();

        private string GerarJustificativa()
        {
            return CategoriaRecomendada switch
            {
                0 => "Baseado no seu perfil profissional e objetivos de carreira",
                1 => "Considerando seu bem-estar e consistência nos check-ins",
                2 => "Focando no desenvolvimento pessoal e autoconhecimento",
                3 => "Alinhado com seus objetivos de aprendizado contínuo",
                4 => "Para fortalecer sua segurança financeira",
                5 => "Para investir em relacionamentos significativos",
                6 => "Importante para equilíbrio entre vida pessoal e profissional",
                _ => "Recomendação personalizada para seu crescimento"
            };
        }
    }
}