using System;
using System.ComponentModel.DataAnnotations;
using TTPService.Enums;

namespace TTPService.Validators.Properties
{
    public class ExperimentTypeValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value != null && !(value is string))
                {
                    return new ValidationResult("experimentType should be a string");
                }

                var status = value as string;
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse(typeof(ExperimentType), status, true, out _))
                    {
                        return ValidationResult.Success;
                    }

                    var enumValuesStr = string.Join(',', Enum.GetNames(typeof(ExperimentType)));
                    return new ValidationResult($"Invalid status. valid statuses for experiment-group are: {enumValuesStr}");
                }

                return new ValidationResult("experimentType cant be null or empty");
            }
            catch (Exception)
            {
                return new ValidationResult("Failed to validate experimentType property");
            }
        }
    }
}