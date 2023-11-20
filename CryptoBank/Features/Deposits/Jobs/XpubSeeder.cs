using CryptoBank.Database;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using CryptoBank.Features.Deposits.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace CryptoBank.Features.Deposits.Jobs;

public class XpubSeeder : IHostedService
{
    private readonly ILogger<XpubSeeder> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BitcoinNetworkService _bitcoinNetworkService;

    public XpubSeeder(
        IOptions<DepositsOptions> depositsOptions,
        ILogger<XpubSeeder> logger,
        IServiceScopeFactory serviceScopeFactory,
        BitcoinNetworkService bitcoinNetworkService)
    {
        _depositsOptions = depositsOptions.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _bitcoinNetworkService = bitcoinNetworkService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<Context>();

        var xpubExist = await context.Xpubs.AnyAsync(cancellationToken);

        if (xpubExist)
            return;

        var masterPubKey = GenerateMasterPubKey();

        var currency = await context.Currencies
            .SingleOrDefaultAsync(x => x.Code == _depositsOptions.Currency!.Code, cancellationToken);

        var xpub = new Xpub
        {
            CurrencyId = currency!.Id,
            Value = masterPubKey
        };

        await context.Xpubs.AddAsync(xpub, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Xpub {xpub.Value} added", xpub.Value);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private string GenerateMasterPubKey()
    {
        var network = _bitcoinNetworkService.GetNetwork();

        ExtKey masterKey = new();

        ExtPubKey masterPubKey = masterKey.Neuter();
        var masterPubkey = masterPubKey.ToString(network);

        return masterPubkey;
    }
}
