namespace MyERP.Services.Sales.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string entity, Guid id)
            : base($"{entity} with ID '{id}' not found", 404)
        {
        }

        public NotFoundException(string message)
            : base(message, 404)
        {
        }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409)
        {
        }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, 400)
        {
        }
    }

    public class ValidationException : AppException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("Validation failed", 400)
        {
            Errors = errors;
        }
    }
}
