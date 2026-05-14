using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    /// <summary>
    /// Validates that a numeric value is within a specified range
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NumericRangeAttribute : ValidationAttribute
    {
        private readonly int _minimum;
        private readonly int _maximum;

        public NumericRangeAttribute(int minimum, int maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Null is allowed (optional field)

            if (!int.TryParse(value.ToString(), out int numericValue))
                return new ValidationResult("Value must be a valid number");

            if (numericValue < _minimum || numericValue > _maximum)
            {
                return new ValidationResult(
                    $"{validationContext.DisplayName} must be between {_minimum} and {_maximum}",
                    new[] { validationContext.MemberName ?? "" });
            }

            return ValidationResult.Success;
        }
    }
}
