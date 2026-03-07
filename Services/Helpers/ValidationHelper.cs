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
        var validator = (IValidator<T>?)
            Activator.CreateInstance(
                AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .First(t => !t.IsAbstract && validatorType.IsAssignableFrom(t))
            );

        if (validator is null)
            throw new InvalidOperationException($"No validator found for {typeof(T).Name}");

        var result = validator.Validate(obj);
        if (!result.IsValid)
            throw new ArgumentException(result.Errors.First().ErrorMessage);
    }
}
