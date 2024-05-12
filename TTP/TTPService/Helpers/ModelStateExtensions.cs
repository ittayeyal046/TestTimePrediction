using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TTPService.Helpers;

public static class ModelStateExtensions
{
    /// <summary>
    /// This method return object that represents 422 error.
    /// </summary>
    /// <param name="modelState">The model-state.</param>
    /// <returns> Unprocessable entity objectResult. </returns>
    public static UnprocessableEntityObjectResult CreateValidationError(this ModelStateDictionary modelState)
    {
        return new UnprocessableEntityObjectResult(modelState);
    }
}