using CryptoBank.Database;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace CryptoBank.Features.Deposits.Jobs;

public class XpubSeeder : IHostedService
{
    private readonly ILogger<XpubSeeder> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public XpubSeeder(
        IOptions<DepositsOptions> depositsOptions,
        ILogger<XpubSeeder> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _depositsOptions = depositsOptions.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
        var network = GetNetwork();

        ExtKey masterKey = new();
        var masterPrvKey = masterKey.ToString(network);

        _logger.LogInformation("Private key: {masterPrvKey}", masterPrvKey);

        ExtPubKey masterPubKey = masterKey.Neuter();
        var masterPubkey = masterPubKey.ToString(network);

        _logger.LogInformation("Public key: {masterPubkey}", masterPubkey);

        return masterPubkey;
    }

    [Obsolete("TODO дубль, переместить в сервисы")]
    private Network GetNetwork()
    {
        return _depositsOptions.BitcoinNetwork switch
        {
            BitcoinNetwork.MainNet => Network.Main,
            BitcoinNetwork.TestNet => Network.TestNet,
            BitcoinNetwork.RegTest => Network.RegTest,
            _ => throw new ArgumentOutOfRangeException(nameof(_depositsOptions.BitcoinNetwork)),
        };
    }
}
