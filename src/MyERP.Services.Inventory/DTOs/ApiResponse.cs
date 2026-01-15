namespace MyERP.Services.Inventory.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, string message, T? data, int statusCode)
        {
            Success = success;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }

        public static ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T>(false, message, default, statusCode);
        }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, message, data, 200);
        }
    }

    // Non-generic version for simple messages
    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse Fail(string message, int statusCode = 400)
        {
            return new ApiResponse { 
                Success = false, 
                Message = message, 
                Data = null, 
                StatusCode = statusCode };
        }
        
        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse { 
                Success = true, 
                Message = message, 
                Data = null, 
                StatusCode = 200 };
        }
    }
}
