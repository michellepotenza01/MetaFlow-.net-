using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.Common
{
    [SwaggerSchema("Parâmetros de paginação para endpoints de listagem")]
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;

        [SwaggerSchema("Número da página (base 1)")]
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [SwaggerSchema("Quantidade de itens por página (máximo 100)")]
        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }

    [SwaggerSchema("Resposta paginada com links HATEOAS")]
    public class PagedResponse<T>
    {
        [SwaggerSchema("Dados da página atual")]
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = new List<T>();

        [SwaggerSchema("Número da página atual")]
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [SwaggerSchema("Quantidade de itens por página")]
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [SwaggerSchema("Total de registros encontrados")]
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [SwaggerSchema("Total de páginas disponíveis")]
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [SwaggerSchema("Links de navegação HATEOAS")]
        [JsonPropertyName("links")]
        public List<Link> Links { get; set; } = new List<Link>();

        [SwaggerSchema("Mensagem descritiva")]
        [JsonPropertyName("message")]
        public string Message { get; set; } = "Dados paginados recuperados com sucesso";

        public PagedResponse() { }

        public PagedResponse(List<T> data, int page, int pageSize, int totalCount, List<Link> links, string message = "Dados paginados recuperados com sucesso")
        {
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            Links = links;
            Message = message;
        }
    }
}