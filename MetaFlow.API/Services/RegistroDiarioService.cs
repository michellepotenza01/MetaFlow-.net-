using MetaFlow.API.Models;
using MetaFlow.API.DTOs;
using MetaFlow.API.Repositories;
using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Services
{
    public interface IRegistroDiarioService
    {
        Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosPagedAsync(PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams);
        Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosByUsuarioAndPeriodoPagedAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim, PaginationParams paginationParams);
        Task<ServiceResponse<List<RegistroDiario>>> GetRegistrosByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<RegistroDiario>> GetRegistroByIdAsync(Guid id);
        Task<ServiceResponse<RegistroDiario>> GetRegistroByUsuarioAndDataAsync(Guid usuarioId, DateTime data);
        Task<ServiceResponse<RegistroDiario>> CreateRegistroAsync(Guid usuarioId, RegistroDiarioRequestDto registroDto);
        Task<ServiceResponse<RegistroDiario>> UpdateRegistroAsync(Guid id, RegistroDiarioRequestDto registroDto);
        Task<ServiceResponse<bool>> DeleteRegistroAsync(Guid id);
        Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId);
        Task<ServiceResponse<List<RegistroDiario>>> GetUltimosRegistrosAsync(Guid usuarioId, int quantidade);
    }

    public class RegistroDiarioService : IRegistroDiarioService
    {
        private readonly IRegistroDiarioRepository _registroRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public RegistroDiarioService(IRegistroDiarioRepository registroRepository, IUsuarioRepository usuarioRepository)
        {
            _registroRepository = registroRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _registroRepository.GetAllPagedAsync(paginationParams);
                var pagedResponse = new PagedResponse<RegistroDiario>(result.Registros, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<RegistroDiario>>.Ok(pagedResponse, "Registros diários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<RegistroDiario>>.Error($"Erro ao buscar registros: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosByUsuarioPagedAsync(Guid usuarioId, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<RegistroDiario>>.NotFound("Usuário");

                var result = await _registroRepository.GetByUsuarioPagedAsync(usuarioId, paginationParams);
                var pagedResponse = new PagedResponse<RegistroDiario>(result.Registros, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<RegistroDiario>>.Ok(pagedResponse, "Registros do usuário recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<RegistroDiario>>.Error($"Erro ao buscar registros do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<RegistroDiario>>> GetRegistrosByUsuarioAndPeriodoPagedAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim, PaginationParams paginationParams)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<PagedResponse<RegistroDiario>>.NotFound("Usuário");

                if (dataInicio > dataFim)
                    return ServiceResponse<PagedResponse<RegistroDiario>>.Error("Data de início não pode ser maior que data de fim");

                var result = await _registroRepository.GetByUsuarioAndPeriodoPagedAsync(usuarioId, dataInicio, dataFim, paginationParams);
                var pagedResponse = new PagedResponse<RegistroDiario>(result.Registros, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<RegistroDiario>>.Ok(pagedResponse, "Registros do período recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<RegistroDiario>>.Error($"Erro ao buscar registros do período: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<RegistroDiario>>> GetRegistrosByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<RegistroDiario>>.NotFound("Usuário");

                var registros = await _registroRepository.GetByUsuarioAsync(usuarioId);
                return ServiceResponse<List<RegistroDiario>>.Ok(registros, "Registros do usuário recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<RegistroDiario>>.Error($"Erro ao buscar registros do usuário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<RegistroDiario>> GetRegistroByIdAsync(Guid id)
        {
            try
            {
                var registro = await _registroRepository.GetByIdAsync(id);
                return registro is null
                    ? ServiceResponse<RegistroDiario>.NotFound("Registro diário")
                    : ServiceResponse<RegistroDiario>.Ok(registro, "Registro encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<RegistroDiario>.Error($"Erro ao buscar registro: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<RegistroDiario>> GetRegistroByUsuarioAndDataAsync(Guid usuarioId, DateTime data)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<RegistroDiario>.NotFound("Usuário");

                var registro = await _registroRepository.GetByUsuarioAndDataAsync(usuarioId, data);
                return registro is null
                    ? ServiceResponse<RegistroDiario>.NotFound("Registro para esta data")
                    : ServiceResponse<RegistroDiario>.Ok(registro, "Registro da data encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<RegistroDiario>.Error($"Erro ao buscar registro da data: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<RegistroDiario>> CreateRegistroAsync(Guid usuarioId, RegistroDiarioRequestDto registroDto)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<RegistroDiario>.NotFound("Usuário");

                if (registroDto.Data.Date > DateTime.Now.Date)
                    return ServiceResponse<RegistroDiario>.Error("A data do registro não pode ser futura");

                var registroExistente = await _registroRepository.GetByUsuarioAndDataAsync(usuarioId, registroDto.Data);
                if (registroExistente is not null)
                    return ServiceResponse<RegistroDiario>.Conflict("Já existe um registro para esta data");

                var registro = new RegistroDiario
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    Data = registroDto.Data.Date,
                    Humor = registroDto.Humor,
                    Produtividade = registroDto.Produtividade,
                    TempoFoco = registroDto.TempoFoco,
                    Anotacoes = registroDto.Anotacoes?.Trim(),
                    CriadoEm = DateTime.Now
                };

                await _registroRepository.AddAsync(registro);
                return ServiceResponse<RegistroDiario>.Ok(registro, "Registro diário criado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<RegistroDiario>.Error($"Erro ao criar registro: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<RegistroDiario>> UpdateRegistroAsync(Guid id, RegistroDiarioRequestDto registroDto)
        {
            try
            {
                var registroExistente = await _registroRepository.GetByIdAsync(id);
                if (registroExistente is null)
                    return ServiceResponse<RegistroDiario>.NotFound("Registro diário");

                if (registroDto.Data.Date > DateTime.Now.Date)
                    return ServiceResponse<RegistroDiario>.Error("A data do registro não pode ser futura");

                var conflitoData = await _registroRepository.GetByUsuarioAndDataAsync(registroExistente.UsuarioId, registroDto.Data);
                if (conflitoData is not null && conflitoData.Id != id)
                    return ServiceResponse<RegistroDiario>.Conflict("Já existe outro registro para esta data");

                registroExistente.Data = registroDto.Data.Date;
                registroExistente.Humor = registroDto.Humor;
                registroExistente.Produtividade = registroDto.Produtividade;
                registroExistente.TempoFoco = registroDto.TempoFoco;
                registroExistente.Anotacoes = registroDto.Anotacoes?.Trim();

                await _registroRepository.UpdateAsync(registroExistente);
                return ServiceResponse<RegistroDiario>.Ok(registroExistente, "Registro diário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<RegistroDiario>.Error($"Erro ao atualizar registro: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteRegistroAsync(Guid id)
        {
            try
            {
                var registro = await _registroRepository.GetByIdAsync(id);
                if (registro is null)
                    return ServiceResponse<bool>.NotFound("Registro diário");

                await _registroRepository.DeleteAsync(registro);
                return ServiceResponse<bool>.Ok(true, "Registro diário excluído com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao excluir registro: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<object>> GetEstatisticasByUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<object>.NotFound("Usuário");

                var totalRegistros = await _registroRepository.GetTotalRegistrosByUsuarioAsync(usuarioId);
                var mediaHumor = await _registroRepository.GetMediaHumorByUsuarioAsync(usuarioId);
                var mediaProdutividade = await _registroRepository.GetMediaProdutividadeByUsuarioAsync(usuarioId);
                var ultimosRegistros = await _registroRepository.GetUltimosRegistrosAsync(usuarioId, 7);

                var estatisticas = new
                {
                    TotalRegistros = totalRegistros,
                    MediaHumor = Math.Round(mediaHumor, 2),
                    MediaProdutividade = Math.Round(mediaProdutividade, 2),
                    DiasConsecutivos = CalcularDiasConsecutivos(ultimosRegistros),
                    TendenciaHumor = CalcularTendencia(ultimosRegistros.Select(r => (decimal)r.Humor).ToList()),
                    TendenciaProdutividade = CalcularTendencia(ultimosRegistros.Select(r => (decimal)r.Produtividade).ToList())
                };

                return ServiceResponse<object>.Ok(estatisticas, "Estatísticas de registros recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Erro ao buscar estatísticas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<RegistroDiario>>> GetUltimosRegistrosAsync(Guid usuarioId, int quantidade)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario is null)
                    return ServiceResponse<List<RegistroDiario>>.NotFound("Usuário");

                var registros = await _registroRepository.GetUltimosRegistrosAsync(usuarioId, quantidade);
                return ServiceResponse<List<RegistroDiario>>.Ok(registros, "Últimos registros recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<RegistroDiario>>.Error($"Erro ao buscar últimos registros: {ex.Message}");
            }
        }

        private int CalcularDiasConsecutivos(List<RegistroDiario> registros)
        {
            if (!registros.Any()) return 0;

            var datasOrdenadas = registros.Select(r => r.Data.Date).OrderByDescending(d => d).ToList();
            var diasConsecutivos = 1;

            for (int i = 1; i < datasOrdenadas.Count; i++)
            {
                if ((datasOrdenadas[i - 1] - datasOrdenadas[i]).Days == 1)
                    diasConsecutivos++;
                else
                    break;
            }

            return diasConsecutivos;
        }

        private string CalcularTendencia(List<decimal> valores)
        {
            if (valores.Count < 2) return "Estável";

            var primeiro = valores.First();
            var ultimo = valores.Last();

            if (ultimo > primeiro + 1) return "Melhorando";
            if (ultimo < primeiro - 1) return "Piorando";
            return "Estável";
        }
    }
}
