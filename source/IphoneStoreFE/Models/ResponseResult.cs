namespace IphoneStoreFE.Models
{
    /// <summary>
    /// Lớp phản hồi chung từ API Backend.
    /// Dùng để deserialize JSON response.
    /// </summary>
    public class ResponseResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }

        // Constructor cho JSON deserialization
        public ResponseResult()
        {
        }

        public ResponseResult(bool success, string message, T? data = default, string? errorCode = null)
        {
            Success = success;
            Message = message;
            Data = data;
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// ResponseResult không có dữ liệu (non-generic version)
    /// </summary>
    public class ResponseResult : ResponseResult<object>
    {
        public ResponseResult() : base()
        {
        }

        public ResponseResult(bool success, string message, object? data = null, string? errorCode = null)
            : base(success, message, data, errorCode)
        {
        }
    }
}