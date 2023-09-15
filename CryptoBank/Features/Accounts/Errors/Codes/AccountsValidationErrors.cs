namespace CryptoBank.Features.Accounts.Errors.Codes;

public static class AccountsValidationErrors
{
    private const string Prefix = "accounts_validation_";

    public const string UserNotFound = Prefix + "user_not_found";
    public const string AccountsNotFound = Prefix + "accounts_not_found";
}