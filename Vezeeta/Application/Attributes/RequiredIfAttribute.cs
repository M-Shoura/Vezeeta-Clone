using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Attributes
{


public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _expectedValue;

    public RequiredIfAttribute(string propertyName, object expectedValue)
    {
        _propertyName = propertyName;
        _expectedValue = expectedValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var property = ctx.ObjectType.GetProperty(_propertyName)
            ?? throw new ArgumentException($"Property '{_propertyName}' not found.");

        var actualValue = property.GetValue(ctx.ObjectInstance);

        if (!_expectedValue.Equals(actualValue))
            return ValidationResult.Success;

        if (value is null || (value is string s && string.IsNullOrWhiteSpace(s)))
            return new ValidationResult(ErrorMessage ?? $"{ctx.DisplayName} is required.");

        return ValidationResult.Success;
    }
    }
}
