/*
 * Custom Exceptions
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: throw new Exception("Not found")
 * ✅ INDUSTRY:
 *    1. Custom exception types for different scenarios
 *    2. Middleware catches and converts to proper HTTP responses
 *    3. Consistent error format across all APIs
 */

namespace MyERP.Services.Production.Exceptions
{
    /// <summary>
    /// Base application exception with status code
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }
        
        public AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Resource not found (404)
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string entity, object key) 
            : base($"{entity} with key '{key}' was not found.", 404)
        {
        }
        
        public NotFoundException(string message) 
            : base(message, 404)
        {
        }
    }

    /// <summary>
    /// Validation error (400)
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(string message) 
            : base(message, 400)
        {
        }
    }

    /// <summary>
    /// Business rule violation (422)
    /// </summary>
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message) 
            : base(message, 422)
        {
        }
    }

    /// <summary>
    /// Conflict - resource already exists (409)
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message) 
            : base(message, 409)
        {
        }
    }
}
