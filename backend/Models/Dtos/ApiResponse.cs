//DTO for API like the template as we want to show to users
namespace backend.Models.Dtos
{
    //This is generic APi Response with T is any types as parameters
    public class ApiResponse<T>
    {
        public required int Status { get; set; }
        public required string Message { get; set; }
        public T? Result { get; set; }

        public static ApiResponse<T> Success(T result, string message = "Success", int status = 200) => new()
        {
            Status = status,
            Message = message,
            Result = result,
        };

        public static ApiResponse<T> Fail(string message, int status = 400) => new()
        {
            Status = status,
            Message = message,
            Result = default,
        };
    }

    //This is implemented ApiResponse

    public class ApiResponse
    {
        public required int Status { get; set; }
        public required string Message { get; set; }

        public static ApiResponse Success(string message = "Success", int status = 200) => new()
        {
            Status = status,
            Message = message,
        };

        public static ApiResponse Fail(string message, int status = 400) => new()
        {
            Status = status,
            Message = message,
        };
    }
}
