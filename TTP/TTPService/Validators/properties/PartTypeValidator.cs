using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TTPService.Validators.Properties
{
    public class PartTypeValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value != null && !(value is string))
                {
                    return new ValidationResult("partType should be a string");
                }

                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    return new ValidationResult("partType cant be null or empty");
                }

                var partType = value.ToString();

                if (partType.Length > 21)
                {
                    return new ValidationResult("partType maximum length cant exceed 21 characters");
                }

                // Examples that express the regular expression:
                // 1. HH 6HY789  J
                // 2. HH 6HY789 A J
                var regExPattern = "^[a-zA-Z\\d]{2}\\s[a-zA-Z\\d]{6}\\s(^$)|([a-zA-Z\\d]{1})\\s[a-zA-Z\\d]{1}\\s(^$)|([a-zA-Z\\d]{2})\\s(^$)|([a-zA-Z\\d]{4})?$";
                var result = Regex.IsMatch(
                    partType,
                    regExPattern);

                if (!result)
                {
                    return new ValidationResult("partType is invalid");
                }

                return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("Failed to validate partType property");
            }
        }
    }
}
