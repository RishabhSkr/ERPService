namespace MyERP.Services.Inventory.Exceptions
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
        public NotFoundException(string message) : base(message, 404) { }
        
        public NotFoundException(string entityName, Guid id) 
            : base($"{entityName} with ID {id} not found", 404) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, 400) { }
    }

    public class InsufficientStockException : AppException
    {
        public List<InsufficientMaterialInfo> InsufficientMaterials { get; }

        public InsufficientStockException(string message, List<InsufficientMaterialInfo>? materials = null) 
            : base(message, 400)
        {
            InsufficientMaterials = materials ?? new List<InsufficientMaterialInfo>();
        }
    }

    public class InsufficientMaterialInfo
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal AvailableStock { get; set; }
    }
}
