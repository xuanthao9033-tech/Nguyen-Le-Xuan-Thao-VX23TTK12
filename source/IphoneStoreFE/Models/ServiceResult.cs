namespace IphoneStoreFE.Models
{
    /// <summary>
    /// Service result wrapper for consistent API responses
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ServiceResult()
        {
        }

        private ServiceResult(bool isSuccess, string message, T? data = default)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Create successful result
        /// </summary>
        public static ServiceResult<T> Success(T? data, string message = "Success")
        {
            return new ServiceResult<T>(true, message, data);
        }

        /// <summary>
        /// Create failure result
        /// </summary>
        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>(false, message, default);
        }
    }
}

