namespace CryptoBank.Features.Deposits.Errors.Codes;

public static class DepositsLogicConflictErrors
{
    private const string Prefix = "deposits_logic_conflict_";

    public const string CurrencyNotExist = Prefix + "currency_not_exist";
    public const string XpubNotExist = Prefix + "xpub_not_exist";
    public const string DerivationIndexNotExist = Prefix + "derivation_index_not_exist";
}
