using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class TimeAfterAttribute : ValidationAttribute
    {
        private readonly string _startTimeProperty;

        public TimeAfterAttribute(string startTimeProperty)
        {
            _startTimeProperty = startTimeProperty;
            ErrorMessage = "The field {0} must be after {1}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not TimeSpan endTime)
            {
                return new ValidationResult($"{validationContext.MemberName} must be a time.");
            }

            PropertyInfo? startProperty = validationContext.ObjectType.GetProperty(_startTimeProperty);
            if (startProperty is null)
            {
                return new ValidationResult($"Unknown property '{_startTimeProperty}'.");
            }

            object? startValue = startProperty.GetValue(validationContext.ObjectInstance);
            if (startValue is null)
            {
                return ValidationResult.Success;
            }

            if (startValue is not TimeSpan startTime)
            {
                return new ValidationResult($"{_startTimeProperty} must be a time.");
            }

            if (endTime <= startTime)
            {
                return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName, _startTimeProperty));
            }

            return ValidationResult.Success;
        }
    }
}
