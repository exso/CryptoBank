namespace CryptoBank.Features.Accounts.Errors.Codes;

public static class AccountsLogicConflictErrors
{
    private const string Prefix = "accounts_logic_confict_";

    public const string AllowedNumberOfAccountsLimit = Prefix + "allowed_number_of_accounts_limit";
    public const string InsufficientAmountInTheAccount = Prefix + "insufficient_amount_in_the_account";
}
