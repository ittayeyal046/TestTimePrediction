using FluentValidation;

namespace TTPService.Validators.Properties
{
    public static class PropertyValidatorExtensions
    {
        public const string ErrorMessage = "{PropertyName} {Error}";

        public static void SetError<T>(this ValidationContext<T> context, string error)
        {
            context.MessageFormatter.AppendArgument("Error", error);
        }
    }
}