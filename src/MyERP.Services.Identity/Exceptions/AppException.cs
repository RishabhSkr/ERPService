using System.Globalization;

namespace MyERP.Services.Identity.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; } = 400;

        public AppException() : base() {}

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args) 
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message)
        {
            StatusCode = 404;
        }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized") : base(message)
        {
            StatusCode = 401;
        }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Access Denied") : base(message)
        {
            StatusCode = 403;
        }
    }
}
