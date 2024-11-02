namespace dotInstrukcijeBackend.ServiceResultUtility
{
    // ServiceResult.cs

    public class ServiceResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public int StatusCode { get; }


        protected ServiceResult(bool isSuccess, string errorMessage, int statusCode)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public static ServiceResult Success()
        {
            return new ServiceResult(true, null, 200);
        }

        public static ServiceResult Failure(string errorMessage, int statusCode)
        {
            return new ServiceResult(false, errorMessage, statusCode);
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; }

        protected ServiceResult(bool isSuccess, string errorMessage, int statusCode, T data = default)
            : base(isSuccess, errorMessage, statusCode)
        {
            Data = data;
        }

        public static new ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>(true, null, 200, data);
        }

        public static new ServiceResult<T> Failure(string errorMessage, int statusCode)
        {
            return new ServiceResult<T>(false, errorMessage, statusCode, default);
        }
    }

}
