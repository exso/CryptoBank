using FluentValidation;

namespace CryptoBank.Validation;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidNumber<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty()
            .MinimumLength(30)
            .MaximumLength(256);
    }
}
