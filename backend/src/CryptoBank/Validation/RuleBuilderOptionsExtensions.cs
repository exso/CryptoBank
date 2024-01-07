using FluentValidation;

using static CryptoBank.Features.Accounts.Errors.Codes.AccountsValidationErrors;
using static CryptoBank.Features.Management.Errors.Codes.UserProfileValidationErrors;

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

    public static IRuleBuilderOptions<T, DateTime> ValidDate<T>(this IRuleBuilder<T, DateTime> builder)
    {
        return builder
            .NotEmpty().WithErrorCode(DateNotEmpty);
    }

    public static IRuleBuilderOptions<T, string> ValidLength<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .MinimumLength(5).WithErrorCode(MinimumLength)
            .MaximumLength(20).WithErrorCode(MaximumLength);
    }
}
