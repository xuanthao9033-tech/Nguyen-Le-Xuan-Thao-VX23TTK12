namespace IphoneStoreBE.Common.Models
{
    /// <summary>
    /// Lớp phản hồi chung cho API.
    /// Hỗ trợ generic để đóng gói dữ liệu trả về.
    /// </summary>
    public class ResponseResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }

        // ✅ Public parameterless constructor for JSON deserialization
        public ResponseResult()
        {
        }

        protected ResponseResult(bool success, string message = "", string? errorCode = null, T? data = default)
        {
            Success = success;
            Message = message;
            ErrorCode = errorCode;
            Data = data;
        }

        /// <summary>
        /// Tạo phản hồi thành công (có dữ liệu).
        /// </summary>
        public static ResponseResult<T> Ok(T? data, string message = "Success")
        {
            return new ResponseResult<T>(true, message, null, data);
        }

        /// <summary>
        /// Tạo phản hồi thất bại.
        /// </summary>
        public static ResponseResult<T> Fail(string message = "", string errorCode = "UNKNOWN_ERROR")
        {
            return new ResponseResult<T>(false, message, errorCode, default);
        }

        // ✅ Alias tương thích với các service cũ
        public static ResponseResult<T> SuccessResult(T? data, string message = "Success")
        {
            return Ok(data, message);
        }
    }

    /// <summary>
    /// Tiện ích trả về phản hồi không có dữ liệu (void).
    /// </summary>
    public class ResponseResult : ResponseResult<object>
    {
        // ✅ Public parameterless constructor for JSON deserialization
        public ResponseResult() : base()
        {
        }

        private ResponseResult(bool success, string message = "", string? errorCode = null, object? data = null)
            : base(success, message, errorCode, data)
        {
        }

        /// <summary>
        /// Tạo phản hồi thành công (không có dữ liệu).
        /// </summary>
        public static ResponseResult Ok(string message = "Success")
        {
            return new ResponseResult(true, message, null, null);
        }

        /// <summary>
        /// Tạo phản hồi thất bại (không có dữ liệu).
        /// </summary>
        public new static ResponseResult Fail(string message = "", string errorCode = "UNKNOWN_ERROR")
        {
            return new ResponseResult(false, message, errorCode, null);
        }

        // ✅ Alias tương thích với các service cũ
        public static ResponseResult SuccessResult(string message = "Success")
        {
            return Ok(message);
        }
    }
}
