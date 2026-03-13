using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Services.Helpers;

public class ValidationHelper
{
    public static void Validate<T>(T obj)
        where T : class
    {
        var validatorType = typeof(AbstractValidator<>).MakeGenericType(typeof(T));
        var validatorImplType = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => !t.IsAbstract && validatorType.IsAssignableFrom(t));

        if (validatorImplType is null)
            throw new InvalidOperationException($"No validator found for {typeof(T).Name}");

        var validator = (IValidator<T>?)Activator.CreateInstance(validatorImplType);

        if (validator is null)
            throw new InvalidOperationException($"No validator found for {typeof(T).Name}");

        var result = validator.Validate(obj);
        if (!result.IsValid)
            throw new ArgumentException(result.Errors.First().ErrorMessage);
    }
}
