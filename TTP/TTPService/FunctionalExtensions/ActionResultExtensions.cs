using System;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace TTPService.FunctionalExtensions
{
    public static class ActionResultExtensions
    {
        public static ActionResult<T> ToActionResult<T>(this Result<T, ErrorResult> result, ControllerBase controller)
        {
            ActionResult<T> actionResult = null;
            result
                .OnFailure(errorResult => { actionResult = FailureHandler<T>(controller, errorResult); })
                .OnSuccess(resultValue => actionResult = controller.Ok(resultValue));

            return actionResult;
        }

        private static ActionResult<T> FailureHandler<T>(ControllerBase controller, ErrorResult errorResult)
        {
            ActionResult<T> actionResult;

            switch (errorResult.ErrorType)
            {
                case ErrorTypes.None:
                    // TODO:[Team]<-[Golan] - let middleware handle exception in one place. it also gives us one stop shop for logging
                    throw new Exception(errorResult.Error);
                case ErrorTypes.BadRequest:
                    actionResult = controller.BadRequest();
                    break;
                case ErrorTypes.NotFound:
                    actionResult = controller.NotFound();
                    break;
                case ErrorTypes.ValidationFailed:
                    actionResult = controller.StatusCode(422, errorResult.Error);
                    break;
                case ErrorTypes.RepositoryError:
                    actionResult = controller.StatusCode(500);
                    break;
                case ErrorTypes.QueueError:
                    actionResult = controller.StatusCode(500);
                    break;
                case ErrorTypes.ExternalServerError:
                    actionResult = controller.StatusCode(500);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(errorResult));
            }

            return actionResult;
        }
    }
}