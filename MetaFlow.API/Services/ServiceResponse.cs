using MetaFlow.API.Models.Common;

namespace MetaFlow.API.Services
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<Link> Links { get; set; } = new List<Link>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ServiceResponse() { }

        public ServiceResponse(T data, string message = "")
        {
            Data = data;
            Message = message;
            Success = true;
        }

        public static ServiceResponse<T> Ok(T data, string message = "")
        {
            return new ServiceResponse<T>(data, message);
        }

        public static ServiceResponse<T> Error(string errorMessage)
        {
            var response = new ServiceResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = new List<string> { errorMessage }
            };
            return response;
        }

        public static ServiceResponse<T> NotFound(string resourceName)
        {
            return Error($"{resourceName} não encontrado(a)");
        }

        public static ServiceResponse<T> Conflict(string message)
        {
            return Error(message);
        }

        public static ServiceResponse<T> ValidationError(List<string> errors)
        {
            var response = new ServiceResponse<T>
            {
                Success = false,
                Message = "Erro de validação",
                Errors = errors ?? new List<string>()
            };
            return response;
        }

        public static ServiceResponse<T> Unauthorized(string message = "Acesso não autorizado")
        {
            return Error(message);
        }

        public static ServiceResponse<T> Forbidden(string message = "Acesso proibido")
        {
            return Error(message);
        }

        public ServiceResponse<T> WithLinks(List<Link> links)
        {
            Links = links ?? new List<Link>();
            return this;
        }

        public ServiceResponse<T> WithLink(Link link)
        {
            Links.Add(link);
            return this;
        }

        public bool IsValid => Success && Errors.Count == 0;
        

        public string? FirstError => Errors.FirstOrDefault();

        public ServiceResponse<U> Convert<U>(U? newData = default)
        {
            return new ServiceResponse<U>
            {
                Success = Success,
                Message = Message,
                Data = newData,
                Errors = Errors,
                Links = Links,
                Timestamp = Timestamp
            };
        }
    }
}