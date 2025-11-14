using MetaFlow.API.Models.Common;
using MetaFlow.API.Repositories;

namespace MetaFlow.API.Services
{
    public interface IEstatisticasService
    {
        Task<ServiceResponse<object>> GetEstatisticasGeraisAsync();
        Task<ServiceResponse<object>> GetEstatisticasUsuarioAsync(Guid usuarioId);
        Task<bool> TemDadosSuficientesParaAnaliseAsync(Guid usuarioId);
        Task<ServiceResponse<object>> GetMetricasSistemaAsync();
    }

    public class EstatisticasService : IEstatisticasService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly IRegistroDiarioRepository _registroRepository;

        public EstatisticasService(
            IUsuarioRepository usuarioRepository,
            IMetaRepository metaRepository,
            IRegistroDiarioRepository registroRepository)
        {
            _usuarioRepository = usuarioRepository;
            _metaRepository = metaRepository;
            _registroRepository = registroRepository;
        }

        public async Task<ServiceResponse<object>> GetEstatisticasGeraisAsync()
        {
            try
            {
                var totalUsuarios = await _usuarioRepository.GetTotalUsuariosAsync();
                var totalMetas = await _metaRepository.GetTotalMetasAsync();
                var totalRegistros = await _registroRepository.GetTotalRegistrosAsync();
                
                var metasConcluidas = await _metaRepository.GetTotalMetasConcluidasAsync();
                var usuariosAtivos = await _usuarioRepository.GetTotalUsuariosAtivosAsync();

                if (totalUsuarios == 0)
                {
                    return ServiceResponse<object>.Ok(new
                    {
                        Mensagem = "Sistema ainda não possui usuários cadastrados",
                        TotalUsuarios = 0,
                        Status = "Aguardando Dados"
                    }, "Estatísticas gerais recuperadas");
                }

                var estatisticas = new
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosAtivos = usuariosAtivos,
                    TotalMetas = totalMetas,
                    TotalRegistros = totalRegistros,
                    MetasConcluidas = metasConcluidas,
                    TaxaConclusaoGeral = totalMetas > 0 ? 
                        Math.Round((decimal)metasConcluidas / totalMetas * 100, 2) : 0,
                    MetasPorUsuario = totalUsuarios > 0 ? 
                        Math.Round((decimal)totalMetas / totalUsuarios, 2) : 0,
                    RegistrosPorUsuario = totalUsuarios > 0 ? 
                        Math.Round((decimal)totalRegistros / totalUsuarios, 2) : 0
                };

                return ServiceResponse<object>.Ok(estatisticas, "Estatísticas gerais recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> GetEstatisticasUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var totalMetas = await _metaRepository.CountByUsuarioAsync(usuarioId);
                var totalRegistros = await _registroRepository.GetTotalRegistrosByUsuarioAsync(usuarioId);

                if (totalMetas == 0 && totalRegistros == 0)
                {
                    return ServiceResponse<object>.Ok(new
                    {
                        Mensagem = "Você ainda não possui dados suficientes para análise.",
                        TotalMetas = 0,
                        TotalRegistros = 0,
                        Status = "Dados Insuficientes",
                        AcoesRecomendadas = new[] 
                        {
                            "Criar sua primeira meta",
                            "Fazer check-in diário"
                        }
                    }, "Dados insuficientes para análise detalhada");
                }

                var metasConcluidas = await _metaRepository.CountByUsuarioAsync(
                    usuarioId, m => m.Status == Enums.StatusMeta.Concluida);
                
                var mediaHumor = await _registroRepository.GetMediaHumorByUsuarioAsync(usuarioId);
                var mediaProdutividade = await _registroRepository.GetMediaProdutividadeByUsuarioAsync(usuarioId);

                var ultimos30Dias = DateTime.UtcNow.AddDays(-30);
                var registrosUltimoMes = await _registroRepository.GetByUsuarioAndPeriodoAsync(
                    usuarioId, ultimos30Dias, DateTime.UtcNow);

                var estatisticas = new
                {
                    TotalMetas = totalMetas,
                    MetasConcluidas = metasConcluidas,
                    TotalRegistros = totalRegistros,
                    RegistrosUltimoMes = registrosUltimoMes.Count,
                    MediaHumor = Math.Round(mediaHumor, 2),
                    MediaProdutividade = Math.Round(mediaProdutividade, 2),
                    TaxaConclusao = totalMetas > 0 ? 
                        Math.Round((decimal)metasConcluidas / totalMetas * 100, 2) : 0,
                    Consistencia = await CalcularConsistencia(usuarioId),
                    NivelEngajamento = CalcularNivelEngajamento(totalRegistros, registrosUltimoMes.Count)
                };

                return ServiceResponse<object>.Ok(estatisticas, "Estatísticas do usuário recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas do usuário: {ex.Message}");
            }
        }

        public async Task<bool> TemDadosSuficientesParaAnaliseAsync(Guid usuarioId)
        {
            var totalRegistros = await _registroRepository.GetTotalRegistrosByUsuarioAsync(usuarioId);
            var totalMetas = await _metaRepository.CountByUsuarioAsync(usuarioId);
            
            return totalRegistros >= 5 || totalMetas >= 3;
        }

        public async Task<ServiceResponse<object>> GetMetricasSistemaAsync()
        {
            try
            {
                var estatisticasGerais = await GetEstatisticasGeraisAsync();
                
                var usuariosNovos = await _usuarioRepository.GetNovosUsuariosUltimos7DiasAsync();
                var metasNovas = await _metaRepository.GetNovasMetasUltimos7DiasAsync();

                var metricas = new
                {
                    Estatisticas = estatisticasGerais.Success ? estatisticasGerais.Data : null,
                    NovosUsuariosSemana = usuariosNovos,
                    NovasMetasSemana = metasNovas,
                    Performance = "Estável",
                    UltimaAtualizacao = DateTime.UtcNow
                };

                return ServiceResponse<object>.Ok(metricas, "Métricas do sistema recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Erro ao buscar métricas do sistema: {ex.Message}");
            }
        }

        private async Task<decimal> CalcularConsistencia(Guid usuarioId)
        {
            var registros = await _registroRepository.GetByUsuarioAsync(usuarioId);
            if (!registros.Any()) return 0;

            var datas = registros.Select(r => r.Data.Date).Distinct().Count();
            var primeiraData = registros.Min(r => r.Data);
            var diasTotais = (DateTime.UtcNow - primeiraData).TotalDays;

            return diasTotais > 0 ? Math.Round((decimal)(datas / diasTotais * 100), 2) : 0;
        }

        private string CalcularNivelEngajamento(int totalRegistros, int registrosUltimoMes)
        {
            if (totalRegistros == 0) return "Iniciante";
            if (registrosUltimoMes >= 20) return "Alto";
            if (registrosUltimoMes >= 10) return "Médio";
            return "Baixo";
        }
    }
}