using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TTPService.Validators.Properties
{
    public class PathValidator : ValidationAttribute
    {
        private readonly PathType _pathType;

        public PathValidator()
        {
            _pathType = PathType.Unc;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value != null && !(value is string))
                {
                    return new ValidationResult("tpPath should be a string");
                }

                var path = value.ToString();

                var invalidChars = Path.GetInvalidPathChars();
                if (invalidChars.Any(i => path.Contains(i)))
                {
                    return new ValidationResult("tpPath contains invalid characters");
                }

                var pattern = GetRegexPattern(_pathType);

                if (!Regex.IsMatch(path.Trim(), pattern))
                {
                    return new ValidationResult("tpPath is not valid");
                }

                return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("Failed to validate tpPath property");
            }
        }

        private string GetRegexPattern(PathType pathType)
        {
            return pathType switch
            {
                PathType.Default => @"^(?:[a-zA-Z]\:|\\\\[\w\-\.]+)\\",
                PathType.Unc => @"^(?:\\\\[\w\-\.]+)\\",
                PathType.Drive => @"^(?:[a-zA-Z]\:)\\",
                _ => throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null),
            };
        }
    }
}
