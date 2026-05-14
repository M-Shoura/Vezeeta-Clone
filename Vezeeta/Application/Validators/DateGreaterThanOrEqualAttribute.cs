using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    /// <summary>
    /// Validates that a date is greater than or equal to a comparison date
    /// Used for validating that ResolvedDate is not earlier than DiagnosedDate
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanOrEqualAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanOrEqualAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Null is allowed (optional field)

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                return new ValidationResult($"Property '{_comparisonProperty}' not found");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue == null)
                return ValidationResult.Success;

            if (!(value is DateTime dateValue))
                return new ValidationResult("Invalid date format");

            if (!(comparisonValue is DateTime comparisonDate))
                return new ValidationResult("Invalid comparison date format");

            if (dateValue < comparisonDate)
            {
                return new ValidationResult(
                    $"{validationContext.DisplayName} cannot be earlier than {_comparisonProperty}",
                    new[] { validationContext.MemberName ?? "" });
            }

            return ValidationResult.Success;
        }
    }
}
