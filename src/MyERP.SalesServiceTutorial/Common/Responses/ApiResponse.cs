
namespace MyERP.SalesServiceTutorial.Common.Responses;
// non-generic base class
public class ApiResponse
{
    public bool Success { get; protected set; }
    public string? Message { get; protected set; }
    // TODO: Static method Success(message)
     public static ApiResponse Ok(string msg)
     {
        return new ApiResponse 
        {
            Success = true,
            Message = msg
        };
     }
    // TODO: Static method Fail(message)
    public static ApiResponse Fail(string msg)
    {
        return new ApiResponse
        {
            Success = false,
            Message = msg
        };
    }
    
}

// generic class
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; private set; }
    //Static method Ok(message, data)
    public static new ApiResponse<T> Ok(string msg, T data){
        return new ApiResponse<T>{
            Success = true,
            Message = msg,
            Data = data
        };
    }
    // Static method Fail(message, data)
    public static new ApiResponse<T> Fail(string msg)
    {
        return new ApiResponse<T>{
            Success = false,
            Message = msg,
            Data = default // null or default value
        };
    }

}