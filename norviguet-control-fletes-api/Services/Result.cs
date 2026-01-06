namespace norviguet_control_fletes_api.Services
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }

        public static Result<T> Success(T data) => new() 
        { 
            IsSuccess = true, 
            Data = data 
        };

        public static Result<T> Failure(string errorMessage, string? errorCode = null) => new() 
        { 
            IsSuccess = false, 
            ErrorMessage = errorMessage, 
            ErrorCode = errorCode 
        };
    }
}
