namespace TTPService.FunctionalExtensions
{
    public class ErrorResult
    {
        public string Error { get; set; }

        public ErrorTypes ErrorType { get; set; }

        public static ErrorResult TypedError(ErrorTypes errorType, string error = "") =>
            new ErrorResult
            {
                ErrorType = errorType,
                Error = error,
            };
    }
}
