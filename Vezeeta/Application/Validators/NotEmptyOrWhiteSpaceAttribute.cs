using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NotEmptyOrWhiteSpaceAttribute : ValidationAttribute
    {
        public NotEmptyOrWhiteSpaceAttribute()
        {
            ErrorMessage = "The field {0} cannot be empty or whitespace.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            return value is string text && !string.IsNullOrWhiteSpace(text);
        }
    }
}
