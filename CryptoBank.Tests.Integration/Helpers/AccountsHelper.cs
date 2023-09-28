using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Tests.Integration.Helpers;

public static class AccountsHelper
{
    public static (Account account1, Account account2) CreateAccounts(
        User user,
        string currency,
        decimal fromAmount,
        decimal toAmount)
    {
        var account1 = new Account
        {
            Number = "541e377c-9a38-46a5-a02c-0720dcc4cc2e",
            Currency = currency,
            Amount = fromAmount,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        var account2 = new Account
        {
            Number = "a53a4971-2ac7-4f89-9734-84dee9fa1d92",
            Currency = currency,
            Amount = toAmount,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        user.UserAccounts.Add(account1);
        user.UserAccounts.Add(account2);

        return (account1, account2);
    }
}
