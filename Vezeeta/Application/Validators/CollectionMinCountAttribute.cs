using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CollectionMinCountAttribute : ValidationAttribute
    {
        private readonly int _minCount;

        public CollectionMinCountAttribute(int minCount)
        {
            _minCount = minCount;
            ErrorMessage = "The field {0} must contain at least {1} item(s).";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not IEnumerable collection)
            {
                return false;
            }

            int count = 0;
            foreach (object? _ in collection)
            {
                count++;
                if (count >= _minCount)
                {
                    return true;
                }
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, _minCount);
        }
    }
}
