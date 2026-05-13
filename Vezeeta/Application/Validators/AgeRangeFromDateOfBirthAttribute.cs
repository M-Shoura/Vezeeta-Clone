using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AgeRangeFromDateOfBirthAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public AgeRangeFromDateOfBirthAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
            ErrorMessage = "Age must be between {0} and {1}.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not DateTime dateOfBirth)
            {
                return false;
            }

            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (DateTime.Today.DayOfYear < dateOfBirth.DayOfYear)
            {
                age--;
            }

            return age >= _minAge && age <= _maxAge;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, _minAge, _maxAge);
        }
    }
}
