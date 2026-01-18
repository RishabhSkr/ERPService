namespace MyERP.Services.Sales.DTOs
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse { Success = true, Message = message, StatusCode = 200 };
        }

        public static ApiResponse Fail(string message, int statusCode = 400)
        {
            return new ApiResponse { Success = false, Message = message, StatusCode = statusCode };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data, StatusCode = 200 };
        }

        public static new ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode };
        }
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
