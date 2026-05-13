using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DateGreaterThanOrEqualToAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DateGreaterThanOrEqualToAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
            ErrorMessage = "The field {0} must be on or after {1}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not DateTime currentDate)
            {
                return new ValidationResult($"{validationContext.MemberName} must be a date.");
            }

            PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(_otherProperty);
            if (otherProperty is null)
            {
                return new ValidationResult($"Unknown property '{_otherProperty}'.");
            }

            object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
            if (otherValue is null)
            {
                return ValidationResult.Success;
            }

            if (otherValue is not DateTime otherDate)
            {
                return new ValidationResult($"{_otherProperty} must be a date.");
            }

            if (currentDate.Date < otherDate.Date)
            {
                return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName, _otherProperty));
            }

            return ValidationResult.Success;
        }
    }
}
