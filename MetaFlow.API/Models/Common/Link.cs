using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MetaFlow.API.Models.Common
{

[SwaggerSchema("Link HATEOAS")]
    public class Link
    {
        [SwaggerSchema("URI do recurso")]
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        [SwaggerSchema("Relação do link (self, next, prev, ...)")]
        [JsonPropertyName("rel")]
        public string Rel { get; set; } = string.Empty;

        [SwaggerSchema("Método HTTP")]
        [JsonPropertyName("method")]
        public string Method { get; set; } = "GET";

        public Link() { }

        public Link(string href, string rel, string method = "GET")
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
    }
}