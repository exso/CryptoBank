using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.Tests.Integration.Helpers;

public static class DepositsHelper
{
    public static async Task CreateCurrency(TestFixture fixture, CancellationToken cancellationToken)
    {
        var currency = new Currency
        {
            Code = "BTC",
            Name = "Bitcoin"
        };

        await fixture.Database.Execute(async x =>
        {
            await x.Currencies.AddAsync(currency, cancellationToken);
            await x.SaveChangesAsync(cancellationToken);
        });
    }

    public static async Task CreateVariable(TestFixture fixture, CancellationToken cancellationToken)
    {
        var variable = new Variable
        {
            Key = DerivationIndex.Key,
            Value = 1
        };

        await fixture.Database.Execute(async x =>
        {
            await x.Variables.AddAsync(variable, cancellationToken);
            await x.SaveChangesAsync(cancellationToken);
        });
    }

    public static async Task CreateXpub(TestFixture fixture, CancellationToken cancellationToken)
    {
        var currency = await fixture.Database.Execute(async x =>
            await x.Currencies.SingleOrDefaultAsync(x => x.Code == "BTC", cancellationToken));

        var masterPubKey = GenerateMasterPubKey();

        var xpub = new Xpub
        {
            CurrencyId = currency!.Id,
            Value = masterPubKey
        };

        await fixture.Database.Execute(async x =>
        {
            await x.Xpubs.AddAsync(xpub, cancellationToken);
            await x.SaveChangesAsync(cancellationToken);
        });
    }

    private static string GenerateMasterPubKey()
    {
        var network = Network.TestNet;

        ExtKey masterKey = new();

        ExtPubKey masterPubKey = masterKey.Neuter();
        var masterPubkey = masterPubKey.ToString(network);

        return masterPubkey;
    }
}
