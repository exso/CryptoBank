namespace CryptoBank.Features.Accounts.Errors.Codes;

public static class AccountsValidationErrors
{
    private const string Prefix = "accounts_validation_";

    public const string UserNotFound = Prefix + "user_not_found";
    public const string AccountsNotFound = Prefix + "accounts_not_found";
    public const string StartDateMustBeBeforeEndDate = Prefix + "start_date_must_be_before_end_date";
    public const string DateNotEmpty = Prefix + "date_not_empty";
}