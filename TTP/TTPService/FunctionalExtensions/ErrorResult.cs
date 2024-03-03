namespace TTPService.FunctionalExtensions
{
    public class ErrorResult
    {
        public const string DefaultError = "_"; // this is not used, only given to  Result.Fail<T> to override

        public string Error { get; set; }

        public ErrorTypes ErrorType { get; set; }

        public static ErrorResult TypedError(ErrorTypes errorType, string error = "") =>
            new ErrorResult
            {
                ErrorType = errorType,
                Error = error,
            };

        public static ErrorResult StringError(string error) =>
            new ErrorResult { Error = error };
    }
}
