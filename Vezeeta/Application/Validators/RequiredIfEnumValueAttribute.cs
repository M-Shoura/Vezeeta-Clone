using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfEnumValueAttribute : ValidationAttribute
    {
        private readonly string _enumProperty;
        private readonly object _targetEnumValue;

        public RequiredIfEnumValueAttribute(string enumProperty, object targetEnumValue)
        {
            _enumProperty = enumProperty;
            _targetEnumValue = targetEnumValue;
            ErrorMessage = "The field {0} is required.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            PropertyInfo? enumProperty = validationContext.ObjectType.GetProperty(_enumProperty);
            if (enumProperty is null)
            {
                return new ValidationResult($"Unknown property '{_enumProperty}'.");
            }

            object? enumValue = enumProperty.GetValue(validationContext.ObjectInstance);
            if (enumValue is null)
            {
                return ValidationResult.Success;
            }

            if (enumValue.Equals(_targetEnumValue) && IsMissing(value))
            {
                return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        private static bool IsMissing(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is string text)
            {
                return string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}
