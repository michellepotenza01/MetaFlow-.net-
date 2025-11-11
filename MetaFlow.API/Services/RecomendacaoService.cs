using MetaFlow.API.Models;
using MetaFlow.API.Models.ML;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;
using Microsoft.ML;

namespace MetaFlow.API.Services
{
    public interface IRecomendacaoService
    {
        Task<ServiceResponse<List<RecomendacaoResultado>>> GerarRecomendacoesAsync(Guid usuarioId);
        Task<ServiceResponse<object>> AnalisarPadroesAsync(Guid usuarioId);
        Task<ServiceResponse<object>> PreverProgressoMetaAsync(Guid metaId);
    }

    public class RecomendacaoService : IRecomendacaoService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly IRegistroDiarioRepository _registroRepository;
        private readonly ILogger<RecomendacaoService> _logger;
        private bool _modelTrained = false;

        public RecomendacaoService(
            IUsuarioRepository usuarioRepository,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository,
            ILogger<RecomendacaoService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _metaRepository = metaRepository;
            _registroRepository = registroRepository;
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
            
            InitializeModel();
        }

        private void InitializeModel()
        {
            try
            {
                _logger.LogInformation("Inicializando modelo ML.NET...");

                var trainingData = new[]
                {
                    new RecomendacaoInput 
                    { 
                        CategoriaFrequenteEncoded = 0f,
                        TaxaConclusaoMedia = 0.8f,
                        DuracaoMediaMetas = 30f,
                        ConsistenciaCheckins = 0.9f,
                        MediaProdutividade = 8f,
                        MediaHumor = 7f,
                        CategoriaRecomendada = 0f
                    },
                    new RecomendacaoInput 
                    { 
                        CategoriaFrequenteEncoded = 1f,
                        TaxaConclusaoMedia = 0.6f,
                        DuracaoMediaMetas = 45f,
                        ConsistenciaCheckins = 0.7f,
                        MediaProdutividade = 6f,
                        MediaHumor = 6f,
                        CategoriaRecomendada = 1f
                    },
                    new RecomendacaoInput 
                    { 
                        CategoriaFrequenteEncoded = 2f,
                        TaxaConclusaoMedia = 0.9f,
                        DuracaoMediaMetas = 15f,
                        ConsistenciaCheckins = 0.8f,
                        MediaProdutividade = 9f,
                        MediaHumor = 8f,
                        CategoriaRecomendada = 2f
                    }
                };

                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                var pipeline = _mlContext.Transforms.Concatenate(
                    "Features", 
                    nameof(RecomendacaoInput.CategoriaFrequenteEncoded),
                    nameof(RecomendacaoInput.TaxaConclusaoMedia),
                    nameof(RecomendacaoInput.DuracaoMediaMetas),
                    nameof(RecomendacaoInput.ConsistenciaCheckins),
                    nameof(RecomendacaoInput.MediaProdutividade),
                    nameof(RecomendacaoInput.MediaHumor))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: nameof(RecomendacaoInput.CategoriaRecomendada),
                        featureColumnName: "Features"));

                _model = pipeline.Fit(dataView);
                _modelTrained = true;

                _logger.LogInformation("Modelo ML.NET treinado e inicializado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao treinar modelo ML.NET. Usando fallback.");
                _modelTrained = false;
            }
        }

        public async Task<ServiceResponse<List<RecomendacaoResultado>>> GerarRecomendacoesAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<RecomendacaoResultado>>.NotFound("Usuário");

                var input = await CriarInputDoUsuarioAsync(usuarioId);
                var recomendacoes = new List<RecomendacaoResultado>();

                if (_modelTrained && _model != null)
                {
                    var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecomendacaoInput, RecomendacaoPrediction>(_model);
                    var prediction = predictionEngine.Predict(input);

                    recomendacoes.Add(new RecomendacaoResultado
                    {
                        Categoria = prediction.Categoria,
                        Confianca = prediction.Confianca,
                        Justificativa = prediction.Justificativa,
                        Prioridade = prediction.Confianca > 0.7f ? "Alta" : "Média"
                    });
                }

                recomendacoes.AddRange(GerarRecomendacoesFallback(input));

                _logger.LogInformation("Recomendações geradas para usuário {UsuarioId}: {Count}", usuarioId, recomendacoes.Count);

                return ServiceResponse<List<RecomendacaoResultado>>.Ok(recomendacoes, "Recomendações geradas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar recomendações para usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<RecomendacaoResultado>>.Error($"Erro ao gerar recomendações: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> AnalisarPadroesAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var metas = await _metaRepository.GetByUsuarioAsync(usuarioId);
                var registros = await _registroRepository.GetByUsuarioAsync(usuarioId);

                var padroes = new
                {
                    DiasMaisProdutivos = AnalisarDiasProdutivos(registros),
                    CorrelacaoHumorProdutividade = CalcularCorrelacaoHumorProdutividade(registros),
                    CategoriasMaisBemSucedidas = AnalisarCategoriasSucesso(metas),
                    ConsistenciaCheckins = CalcularConsistenciaCheckins(registros),
                    TendenciaProgresso = AnalisarTendenciaProgresso(metas)
                };

                return ServiceResponse<object>.Ok(padroes, "Padrões analisados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar padrões para usuário {UsuarioId}", usuarioId);
                return ServiceResponse<object>.Error($"Erro ao analisar padrões: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> PreverProgressoMetaAsync(Guid metaId)
        {
            try
            {
                var meta = await _metaRepository.GetByIdAsync(metaId);
                if (meta is null)
                    return ServiceResponse<object>.NotFound("Meta");

                var diasDecorridos = (DateTime.Now - meta.CriadoEm).Days;
                var diasTotais = (meta.Prazo - meta.CriadoEm).Days;
                var progressoEsperado = diasTotais > 0 ? (decimal)diasDecorridos / diasTotais * 100 : 0;

                var previsao = new
                {
                    ProgressoAtual = meta.Progresso,
                    ProgressoEsperado = Math.Round(progressoEsperado, 2),
                    Status = meta.Progresso >= progressoEsperado ? "No prazo" : "Atrasado",
                    DiasRestantes = (meta.Prazo - DateTime.Now).Days,
                    ProbabilidadeConclusao = CalcularProbabilidadeConclusao(meta, progressoEsperado)
                };

                return ServiceResponse<object>.Ok(previsao, "Previsão de progresso gerada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao prever progresso da meta {MetaId}", metaId);
                return ServiceResponse<object>.Error($"Erro ao prever progresso: {ex.Message}");
            }
        }

        private async Task<RecomendacaoInput> CriarInputDoUsuarioAsync(Guid usuarioId)
        {
            var metas = await _metaRepository.GetByUsuarioAsync(usuarioId);
            var registros = await _registroRepository.GetByUsuarioAsync(usuarioId);

            var categoriaFrequente = ObterCategoriaMaisFrequente(metas);
            var taxaConclusao = CalcularTaxaConclusao(metas);
            var duracaoMedia = CalcularDuracaoMediaMetas(metas);
            var consistencia = CalcularConsistenciaCheckins(registros);
            var mediaProdutividade = registros.Any() ? registros.Average(r => r.Produtividade) : 5;
            var mediaHumor = registros.Any() ? registros.Average(r => r.Humor) : 5;

            return new RecomendacaoInput
            {
                UsuarioId = usuarioId,
                CategoriaFrequenteEncoded = (float)categoriaFrequente,
                TaxaConclusaoMedia = (float)taxaConclusao,
                DuracaoMediaMetas = (float)duracaoMedia,
                ConsistenciaCheckins = (float)consistencia,
                MediaProdutividade = (float)mediaProdutividade,
                MediaHumor = (float)mediaHumor
            };
        }

        private List<RecomendacaoResultado> GerarRecomendacoesFallback(RecomendacaoInput input)
        {
            var recomendacoes = new List<RecomendacaoResultado>();

            if (input.MediaProdutividade < 6)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Saúde",
                    Confianca = 0.8f,
                    Justificativa = "Melhorar o bem-estar pode aumentar sua produtividade",
                    Prioridade = "Alta"
                });
            }

            if (input.TaxaConclusaoMedia < 0.5)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Pessoal",
                    Confianca = 0.7f,
                    Justificativa = "Metas menores podem ajudar a construir confiança",
                    Prioridade = "Média"
                });
            }

            if (input.ConsistenciaCheckins < 0.7)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Lazer",
                    Confianca = 0.6f,
                    Justificativa = "Equilíbrio entre trabalho e descanso melhora consistência",
                    Prioridade = "Média"
                });
            }

            return recomendacoes;
        }

        private int ObterCategoriaMaisFrequente(List<Meta> metas)
        {
            if (!metas.Any()) return 0;

            var categorias = metas.GroupBy(m => m.Categoria)
                                .OrderByDescending(g => g.Count())
                                .First()
                                .Key;

            return categorias switch
            {
                "Carreira" => 0,
                "Saúde" => 1,
                "Pessoal" => 2,
                "Educação" => 3,
                "Financeiro" => 4,
                "Relacionamentos" => 5,
                "Lazer" => 6,
                _ => 0
            };
        }

        private decimal CalcularTaxaConclusao(List<Meta> metas)
        {
            var total = metas.Count;
            var concluidas = metas.Count(m => m.Status == "Concluída");
            return total > 0 ? (decimal)concluidas / total : 0;
        }

        private decimal CalcularDuracaoMediaMetas(List<Meta> metas)
        {
            var metasConcluidas = metas.Where(m => m.Status == "Concluída").ToList();
            if (!metasConcluidas.Any()) return 30;

            var mediaDias = metasConcluidas.Average(m => (m.Prazo - m.CriadoEm).TotalDays);
            return (decimal)Math.Max(1, mediaDias);
        }

        private decimal CalcularConsistenciaCheckins(List<RegistroDiario> registros)
        {
            if (!registros.Any()) return 0;

            var diasComRegistro = registros.Select(r => r.Data.Date).Distinct().Count();
            var diasTotais = (DateTime.Now - registros.Min(r => r.Data)).TotalDays;

            return diasTotais > 0 ? (decimal)(diasComRegistro / Math.Max(1, diasTotais)) : 0;
        }

        private string AnalisarDiasProdutivos(List<RegistroDiario> registros)
        {
            if (!registros.Any()) return "Dados insuficientes";

            var diasDaSemana = registros.GroupBy(r => r.Data.DayOfWeek)
                                      .Select(g => new { Dia = g.Key, Media = g.Average(r => r.Produtividade) })
                                      .OrderByDescending(x => x.Media)
                                      .First();

            return $"Melhor produtividade aos {diasDaSemana.Dia}s";
        }

        private decimal CalcularCorrelacaoHumorProdutividade(List<RegistroDiario> registros)
        {
            if (registros.Count < 2) return 0;

            var humor = registros.Select(r => (decimal)r.Humor).ToArray();
            var produtividade = registros.Select(r => (decimal)r.Produtividade).ToArray();

            var mediaHumor = humor.Average();
            var mediaProd = produtividade.Average();

            var covariancia = humor.Zip(produtividade, (h, p) => (h - mediaHumor) * (p - mediaProd)).Sum();
            var varianciaHumor = humor.Sum(h => (h - mediaHumor) * (h - mediaHumor));
            var varianciaProd = produtividade.Sum(p => (p - mediaProd) * (p - mediaProd));

            return varianciaHumor * varianciaProd > 0 ? 
                covariancia / (decimal)Math.Sqrt((double)(varianciaHumor * varianciaProd)) : 0;
        }

        private string AnalisarCategoriasSucesso(List<Meta> metas)
        {
            var categoriasSucesso = metas.Where(m => m.Status == "Concluída")
                                       .GroupBy(m => m.Categoria)
                                       .OrderByDescending(g => g.Count())
                                       .Select(g => g.Key)
                                       .FirstOrDefault();

            return categoriasSucesso ?? "Nenhuma categoria com sucesso significativo";
        }

        private string AnalisarTendenciaProgresso(List<Meta> metas)
        {
            var metasAtivas = metas.Where(m => m.Status == "Ativa").ToList();
            if (!metasAtivas.Any()) return "Sem metas ativas";

            var progressoMedio = metasAtivas.Average(m => m.Progresso);
            return progressoMedio > 50 ? "Bom progresso" : "Progresso lento";
        }

        private string CalcularProbabilidadeConclusao(Meta meta, decimal progressoEsperado)
        {
            var diferenca = meta.Progresso - progressoEsperado;
            return diferenca switch
            {
                > 20 => "Muito Alta",
                > 10 => "Alta",
                > 0 => "Moderada",
                _ => "Baixa"
            };
        }
    }

    public class RecomendacaoResultado
    {
        public string Categoria { get; set; } = string.Empty;
        public float Confianca { get; set; }
        public string Justificativa { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
    }
}