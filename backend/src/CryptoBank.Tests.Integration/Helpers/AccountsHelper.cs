using CryptoBank.Features.Accounts.Domain;
using FluentAssertions;

namespace CryptoBank.Tests.Integration.Helpers;

public static class AccountsHelper
{
    public static (Account account1, Account account2) CreateAccounts(
        int userId,
        string currency,
        decimal fromAmount,
        decimal toAmount)
    {
        var account1 = new Account
        {
            Number = "541e377c-9a38-46a5-a02c-0720dcc4cc2e",
            Currency = currency,
            Amount = fromAmount,
            DateOfOpening = DateTime.Now.ToUniversalTime(),
            UserId = userId
        };

        var account2 = new Account
        {
            Number = "a53a4971-2ac7-4f89-9734-84dee9fa1d92",
            Currency = currency,
            Amount = toAmount,
            DateOfOpening = DateTime.Now.ToUniversalTime(),
            UserId = userId
        };

        return (account1, account2);
    }

    public static (Account account1, Account account2) CreateAccounts(
        int userId,
        string currency,
        decimal amount)
    {
        var account1 = new Account
        {
            Number = "541e377c-9a38-46a5-a02c-0720dcc4cc2e",
            Currency = currency,
            Amount = amount,
            DateOfOpening = DateTime.Now.ToUniversalTime(),
            UserId = userId
        };

        var account2 = new Account
        {
            Number = "a53a4971-2ac7-4f89-9734-84dee9fa1d92",
            Currency = currency,
            Amount = amount,
            DateOfOpening = DateTime.Now.ToUniversalTime(),
            UserId = userId
        };

        return (account1, account2);
    }

    public static void AssertAccount(Account actual, Account expected)
    {
        actual.Should().NotBeNull();
        actual.Number.Should().Be(expected.Number);
        actual.Currency.Should().Be(expected.Currency);
        actual.Amount.Should().Be(expected.Amount);
        actual.DateOfOpening.Date.Should().Be(expected.DateOfOpening.Date);
    }
}
