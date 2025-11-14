using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaFlow.API.Converters
{
    /// <summary>
    /// Converter para DateTime que aceita múltiplos formatos
    /// </summary>
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] SupportedFormats = new[]
        {
            "yyyy-MM-dd",           
            "dd/MM/yyyy",           
            "MM/dd/yyyy",           
            "yyyy-MM-ddTHH:mm:ss",  
            "yyyy-MM-dd HH:mm:ss",  
            "dd/MM/yyyy HH:mm:ss",  
            "MM/dd/yyyy HH:mm:ss",  
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var dateString = reader.GetString()?.Trim();
                    
                    if (string.IsNullOrEmpty(dateString))
                        throw new JsonException("Data não pode ser vazia.");

                    foreach (var format in SupportedFormats)
                    {
                        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                        {
                            return result;
                        }
                    }

                    if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var genericResult))
                    {
                        return genericResult;
                    }

                    throw new JsonException($"Formato de data não suportado: '{dateString}'. Formatos aceitos: {string.Join(", ", SupportedFormats)}");
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    // Suporte a timestamp (segundos desde 1970)
                    if (reader.TryGetInt64(out long timestamp))
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                    }
                }

                throw new JsonException($"Tipo não suportado para conversão de data. Token: {reader.TokenType}");
            }
            catch (Exception ex)
            {
                throw new JsonException($"Erro ao converter data: {ex.Message}");
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    /// <summary>
    /// Converter flexível para DateTime? (nullable)
    /// </summary>
    public class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly FlexibleDateTimeConverter _innerConverter = new();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            return _innerConverter.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                _innerConverter.Write(writer, value.Value, options);
        }
    }
}