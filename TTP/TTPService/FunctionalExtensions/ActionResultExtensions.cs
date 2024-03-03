using System;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace TTPService.FunctionalExtensions
{
    public static class ActionResultExtensions
    {
        public static ActionResult<T> ToActionResult<T>(this Maybe<T> maybe, ControllerBase controller)
        {
            if (maybe.HasNoValue)
            {
                return controller.NotFound();
            }

            return controller.Ok(maybe.Value);
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<T, ActionResult<T>> onSuccessFunc)
        {
            return AsyncResultExtensionsRightOperand.OnFailure(result, s => throw new Exception(s)) // TODO:[Team]<-[Golan] - let middleware handle exception in one place. it also gives us one stop shop for logging
                .OnSuccess(onSuccessFunc) // TODO:[Team]<-[Golan] - Post(create) should return 201 (created) status code with route to newlly created item in header location
                .Result.Value;
        }

        public static ActionResult<VoidResult> ToActionResult(this Result<VoidResult, ErrorResult> result, ControllerBase controller, Func<ControllerBase, ActionResult<VoidResult>> onSuccessFunc)
        {
            ActionResult<VoidResult> actionResult = null;
            result
                .OnFailure(errorResult => { actionResult = FailureHandler<VoidResult>(controller, errorResult); })
                .OnSuccess(voidResult => actionResult = onSuccessFunc(controller));

            return actionResult;
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T, ErrorResult> result, ControllerBase controller, Func<T, ActionResult<T>> onSuccessFunc)
        {
            ActionResult<T> actionResult = null;
            result
                .OnFailure(errorResult => { actionResult = FailureHandler<T>(controller, errorResult); })
                .OnSuccess(resultValue => actionResult = onSuccessFunc(resultValue));

            return actionResult;
        }

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