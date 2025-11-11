using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.Common
{
    [SwaggerSchema("Resposta padrão para erros da API")]
    public class ErrorResponse
    {
        [SwaggerSchema("Mensagem de erro principal")]
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [SwaggerSchema("Lista detalhada de erros de validação")]
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();

        [SwaggerSchema("Código do erro para referência")]
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [SwaggerSchema("Timestamp UTC do erro")]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [SwaggerSchema("Caminho da requisição que causou o erro")]
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [SwaggerSchema("Tipo do erro")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = "ValidationError";

        public ErrorResponse() { }

        public ErrorResponse(string message, List<string>? errors = null, string? errorCode = null, string? path = null, string type = "ValidationError")
        {
            Message = message;
            Errors = errors ?? new List<string>();
            ErrorCode = errorCode;
            Path = path;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }
}


  