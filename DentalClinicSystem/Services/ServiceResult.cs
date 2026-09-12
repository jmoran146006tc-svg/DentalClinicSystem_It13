namespace DentalClinicSystem.Services
{
    public class ServiceResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }

        protected ServiceResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult Ok() => new(true, string.Empty);
        public static ServiceResult Fail(string message) => new(false, message);
    }

    public sealed class ServiceResult<T> : ServiceResult
    {
        public T Data { get; }

        private ServiceResult(bool success, string errorMessage, T data) : base(success, errorMessage)
            => Data = data;

        public static ServiceResult<T> Ok(T data) => new(true, string.Empty, data);
        public static ServiceResult<T> Fail(string message) => new(false, message, default!);
    }
}
