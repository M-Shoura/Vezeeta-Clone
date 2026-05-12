using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NotPastDateAttribute : ValidationAttribute
    {
        public NotPastDateAttribute()
        {
            ErrorMessage = "The field {0} cannot be in the past.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is DateTime date)
            {
                return date.Date >= DateTime.Today;
            }

            return false;
        }
    }
}
