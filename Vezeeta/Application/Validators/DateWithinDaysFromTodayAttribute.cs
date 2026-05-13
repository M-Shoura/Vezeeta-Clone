using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DateWithinDaysFromTodayAttribute : ValidationAttribute
    {
        private readonly int _minDays;
        private readonly int _maxDays;

        public DateWithinDaysFromTodayAttribute(int minDays, int maxDays)
        {
            _minDays = minDays;
            _maxDays = maxDays;
            ErrorMessage = "The field {0} must be between {1} and {2} day(s) from today.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not DateTime date)
            {
                return false;
            }

            int diffDays = (date.Date - DateTime.Today).Days;
            return diffDays >= _minDays && diffDays <= _maxDays;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, _minDays, _maxDays);
        }
    }
}
