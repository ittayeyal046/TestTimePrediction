using CSharpFunctionalExtensions;

namespace TTPService.FunctionalExtensions
{
    public static class ResultExtensions
    {
        public static Result<T, ErrorResult> ToErrorResult<T>(this Result<T> result, ErrorResult errorResult)
        {
            return Result.Fail<T, ErrorResult>(errorResult);
        }

        public static Result<T, ErrorResult> ToErrorResult<T>(this Result<T> result, ErrorTypes errorType)
        {
            return Result.Fail<T, ErrorResult>(ErrorResult.TypedError(errorType, result.IsFailure ? result.Error : string.Empty));
        }

        public static Result<T, ErrorResult> ToErrorResult<T>(this Result result, ErrorTypes errorType)
        {
            return Result.Fail<T, ErrorResult>(ErrorResult.TypedError(errorType, result.IsFailure ? result.Error : string.Empty));
        }

        public static Result<T, ErrorResult> ToNotFoundErrorResult<T>(this Result<T> result)
        {
            return result.ToErrorResult<T>(ErrorTypes.NotFound);
        }

        public static Result<T, ErrorResult> ToBadRequestErrorResult<T>(this Result<T> result)
        {
            return result.ToErrorResult<T>(ErrorTypes.BadRequest);
        }

        public static Result<T, ErrorResult> ToValidationFailedErrorResult<T>(this Result<T> result, string errorMessage)
        {
            return result.ToErrorResult<T>(ErrorResult.TypedError(ErrorTypes.ValidationFailed, errorMessage));
        }

        public static Result<T, ErrorResult> ToRepositoryErrorResult<T>(this Result<T> result)
        {
            return result.ToErrorResult<T>(ErrorTypes.RepositoryError);
        }

        public static Result<T, ErrorResult> ToRepositoryErrorResult<T>(this Result result)
        {
            return result.ToErrorResult<T>(ErrorTypes.RepositoryError);
        }

        public static Result<T, ErrorResult> ToQueueErrorResult<T>(this Result<T> result)
        {
            return result.ToErrorResult<T>(ErrorTypes.QueueError);
        }

        public static Result<T, ErrorResult> ToBadRequestErrorResult<T>(this Result<T> result, string errorMessage)
        {
            return result.ToErrorResult<T>(ErrorResult.TypedError(ErrorTypes.BadRequest, errorMessage));
        }

        public static Result<T, ErrorResult> ToExternalServerErrorResult<T>(this Result<T> result)
        {
            return result.ToErrorResult<T>(ErrorTypes.ExternalServerError);
        }
    }
}
