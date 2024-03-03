using System;
using FluentValidation;

namespace TTPService.Tests.Validators.Properties
{
    public class TestValidator<T> : InlineValidator<TestValidatorData<T>>
    {
        public TestValidator()
        {
        }

        public TestValidator(params Action<TestValidator<T>>[] actions)
        {
            foreach (var action in actions)
            {
                action(this);
            }
        }
    }
}