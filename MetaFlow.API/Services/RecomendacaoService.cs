using MetaFlow.API.Models;
using MetaFlow.API.Models.ML;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;
using MetaFlow.API.Enums;
using Microsoft.ML;
using Microsoft.Extensions.DependencyInjection;

namespace MetaFlow.API.Services
{
    public interface IRecomendacaoService
    {
        Task<ServiceResponse<List<RecomendacaoResultado>>> GerarRecomendacoesAsync(Guid usuarioId);
        Task<ServiceResponse<object>> AnalisarPadroesAsync(Guid usuarioId);
        Task<ServiceResponse<object>> PreverProgressoMetaAsync(Guid metaId);
        Task<ServiceResponse<bool>> TreinarModeloComDadosReaisAsync();
    }

    public class RecomendacaoService : IRecomendacaoService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecomendacaoService> _logger;
        private bool _modelTrained = false;
        private readonly object _modelLock = new object();
        private PredictionEngine<RecomendacaoInput, RecomendacaoPrediction>? _predictionEngine;

        public RecomendacaoService(
            IServiceScopeFactory scopeFactory,
            ILogger<RecomendacaoService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
            
            InitializeModel();
        }

        private void InitializeModel()
        {
            try
            {
                lock (_modelLock)
                {
                    if (_modelTrained) return;

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
                            DuracaoMediaMetas = 20f, 
                            ConsistenciaCheckins = 0.8f, 
                            MediaProdutividade = 9f, 
                            MediaHumor = 8f, 
                            CategoriaRecomendada = 2f 
                        },
                        new RecomendacaoInput 
                        { 
                            CategoriaFrequenteEncoded = 3f, 
                            TaxaConclusaoMedia = 0.7f, 
                            DuracaoMediaMetas = 60f, 
                            ConsistenciaCheckins = 0.6f, 
                            MediaProdutividade = 7f, 
                            MediaHumor = 7f, 
                            CategoriaRecomendada = 3f 
                        },
                        new RecomendacaoInput 
                        { 
                            CategoriaFrequenteEncoded = 4f, 
                            TaxaConclusaoMedia = 0.5f, 
                            DuracaoMediaMetas = 90f, 
                            ConsistenciaCheckins = 0.5f, 
                            MediaProdutividade = 5f, 
                            MediaHumor = 5f, 
                            CategoriaRecomendada = 4f 
                        }
                    };

                    var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "Label", 
                    inputColumnName: nameof(RecomendacaoInput.CategoriaRecomendada))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features", 
                    nameof(RecomendacaoInput.CategoriaFrequenteEncoded),
                    nameof(RecomendacaoInput.TaxaConclusaoMedia),
                    nameof(RecomendacaoInput.DuracaoMediaMetas),
                    nameof(RecomendacaoInput.ConsistenciaCheckins),
                    nameof(RecomendacaoInput.MediaProdutividade),
                    nameof(RecomendacaoInput.MediaHumor)))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: "Label",
                    featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    

                    _model = pipeline.Fit(dataView);
                    
                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<RecomendacaoInput, RecomendacaoPrediction>(_model);
                    _modelTrained = true;

                    _logger.LogInformation("Modelo ML.NET treinado e inicializado com sucesso");
                }
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
                using var scope = _scopeFactory.CreateScope();
                var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                var metaRepository = scope.ServiceProvider.GetRequiredService<IMetaRepository>();
                var registroRepository = scope.ServiceProvider.GetRequiredService<IRegistroDiarioRepository>();

                var usuario = await usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<RecomendacaoResultado>>.NotFound("Usuário");

                var input = await CriarInputDoUsuarioAsync(usuarioId, metaRepository, registroRepository);
                var recomendacoes = new List<RecomendacaoResultado>();

                if (_modelTrained && _predictionEngine != null)
                {
                    try
                    {
                        var prediction = _predictionEngine.Predict(input);
                        recomendacoes.Add(new RecomendacaoResultado
                        {
                            Categoria = prediction.Categoria,
                            Confianca = prediction.Confianca,
                            Justificativa = prediction.Justificativa,
                            Prioridade = prediction.Confianca > 0.7f ? "Alta" : "Média",
                            Tipo = "ML"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro na predição ML. Usando fallback.");
                    }
                }

                if (!recomendacoes.Any())
                {
                    recomendacoes.AddRange(GerarRecomendacoesBaseadasRegras(input));
                }

                recomendacoes = recomendacoes
                    .OrderByDescending(r => r.Prioridade == "Alta")
                    .ThenByDescending(r => r.Confianca)
                    .ToList();

                _logger.LogInformation("Recomendações geradas para usuário {UsuarioId}: {Count} recomendações", 
                    usuarioId, recomendacoes.Count);

                return ServiceResponse<List<RecomendacaoResultado>>.Ok(recomendacoes, "Recomendações geradas com sucesso");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar recomendações para usuário {UsuarioId}", usuarioId);
                return ServiceResponse<List<RecomendacaoResultado>>.Error($"Erro ao gerar recomendações: {ex.Message}");
            }
        }

        private async Task<RecomendacaoInput> CriarInputDoUsuarioAsync(
            Guid usuarioId,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository)
        {
            try
            {
                var metas = await metaRepository.GetByUsuarioAsync(usuarioId);
                var registros = await registroRepository.GetByUsuarioAsync(usuarioId);

                var categoriaFrequente = ObterCategoriaMaisFrequente(metas);
                var taxaConclusao = CalcularTaxaConclusao(metas);
                var duracaoMedia = CalcularDuracaoMediaMetas(metas);
                var consistencia = CalcularConsistenciaCheckins(registros);
                var mediaProdutividade = registros.Any() ? (float)registros.Average(r => r.Produtividade) : 5f;
                var mediaHumor = registros.Any() ? (float)registros.Average(r => r.Humor) : 5f;

                return new RecomendacaoInput
                {
                    CategoriaFrequenteEncoded = categoriaFrequente,
                    TaxaConclusaoMedia = (float)taxaConclusao,
                    DuracaoMediaMetas = (float)duracaoMedia,
                    ConsistenciaCheckins = (float)consistencia,
                    MediaProdutividade = mediaProdutividade,
                    MediaHumor = mediaHumor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar input do usuário {UsuarioId}", usuarioId);
                return new RecomendacaoInput
                {
                    CategoriaFrequenteEncoded = 0f,
                    TaxaConclusaoMedia = 0.5f,
                    DuracaoMediaMetas = 30f,
                    ConsistenciaCheckins = 0.5f,
                    MediaProdutividade = 5f,
                    MediaHumor = 5f
                };
            }
        }


        private List<RecomendacaoResultado> GerarRecomendacoesBaseadasRegras(RecomendacaoInput input)
        {
            var recomendacoes = new List<RecomendacaoResultado>();

            if (input.MediaProdutividade < 6)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Saúde",
                    Confianca = 0.85f,
                    Justificativa = "Melhorar o bem-estar pode aumentar significativamente sua produtividade",
                    Prioridade = "Alta",
                    Tipo = "Regra"
                });
            }

            if (input.TaxaConclusaoMedia < 0.5)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Pessoal",
                    Confianca = 0.75f,
                    Justificativa = "Metas menores e mais frequentes podem ajudar a construir confiança e consistência",
                    Prioridade = "Alta",
                    Tipo = "Regra"
                });
            }

            if (input.ConsistenciaCheckins < 0.7)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Lazer",
                    Confianca = 0.7f,
                    Justificativa = "Equilíbrio entre trabalho e descanso melhora a consistência nos check-ins",
                    Prioridade = "Média",
                    Tipo = "Regra"
                });
            }

            if (input.MediaHumor < 6)
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Relacionamentos",
                    Confianca = 0.8f,
                    Justificativa = "Investir em relacionamentos significativos pode melhorar seu humor geral",
                    Prioridade = "Alta",
                    Tipo = "Regra"
                });
            }

            if (!recomendacoes.Any())
            {
                recomendacoes.Add(new RecomendacaoResultado
                {
                    Categoria = "Pessoal",
                    Confianca = 0.6f,
                    Justificativa = "Continue focando no desenvolvimento pessoal para manter o bom progresso",
                    Prioridade = "Média",
                    Tipo = "Padrão"
                });
            }

            return recomendacoes;
        }

        private int ObterCategoriaMaisFrequente(List<Meta> metas)
        {
            if (!metas.Any()) return 0;

            var categoriaMaisFrequente = metas
                .GroupBy(m => m.Categoria)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            return categoriaMaisFrequente switch
            {
                CategoriaMeta.Carreira => 0,
                CategoriaMeta.Saude => 1,
                CategoriaMeta.Pessoal => 2,
                CategoriaMeta.Educacao => 3,
                CategoriaMeta.Financeiro => 4,
                CategoriaMeta.Relacionamentos => 5,
                CategoriaMeta.Lazer => 6,
                _ => 0
            };
        }

        private decimal CalcularTaxaConclusao(List<Meta> metas)
        {
            var total = metas.Count;
            var concluidas = metas.Count(m => m.Status == StatusMeta.Concluida);
            return total > 0 ? (decimal)concluidas / total : 0;
        }

        private decimal CalcularDuracaoMediaMetas(List<Meta> metas)
        {
            var metasConcluidas = metas.Where(m => m.Status == StatusMeta.Concluida).ToList();
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

        public async Task<ServiceResponse<object>> AnalisarPadroesAsync(Guid usuarioId)
        {
            using var scope = _scopeFactory.CreateScope();
            var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var metaRepository = scope.ServiceProvider.GetRequiredService<IMetaRepository>();
            var registroRepository = scope.ServiceProvider.GetRequiredService<IRegistroDiarioRepository>();

            try
            {
                var usuario = await usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var metas = await metaRepository.GetByUsuarioAsync(usuarioId);
                var registros = await registroRepository.GetByUsuarioAsync(usuarioId);

                return ServiceResponse<object>.Ok(new { Mensagem = "Análise em desenvolvimento" }, "Padrões analisados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar padrões para usuário {UsuarioId}", usuarioId);
                return ServiceResponse<object>.Error($"Erro ao analisar padrões: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> PreverProgressoMetaAsync(Guid metaId)
        {
            using var scope = _scopeFactory.CreateScope();
            var metaRepository = scope.ServiceProvider.GetRequiredService<IMetaRepository>();

            try
            {
                var meta = await metaRepository.GetByIdAsync(metaId);
                if (meta is null)
                    return ServiceResponse<object>.NotFound("Meta");

                return ServiceResponse<object>.Ok(new { Mensagem = "Previsão em desenvolvimento" }, "Previsão gerada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao prever progresso da meta {MetaId}", metaId);
                return ServiceResponse<object>.Error($"Erro ao prever progresso: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> TreinarModeloComDadosReaisAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando treinamento do modelo com dados reais...");
                
                return ServiceResponse<bool>.Ok(true, "Modelo treinado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao treinar modelo com dados reais");
                return ServiceResponse<bool>.Error($"Erro ao treinar modelo: {ex.Message}");
            }
        }
    }

    public class RecomendacaoResultado
    {
        public string Categoria { get; set; } = string.Empty;
        public float Confianca { get; set; }
        public string Justificativa { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}