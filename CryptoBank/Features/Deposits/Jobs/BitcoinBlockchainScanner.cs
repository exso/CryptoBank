using CryptoBank.Database;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using CryptoBank.Features.Deposits.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;
using System.Data;

namespace CryptoBank.Features.Deposits.Jobs;

public class BitcoinBlockchainScanner : BackgroundService
{
    private readonly ILogger<BitcoinBlockchainScanner> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly RPCClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BitcoinNetworkService _bitcoinNetworkService;

    public BitcoinBlockchainScanner(
        ILogger<BitcoinBlockchainScanner> logger,
        IOptions<DepositsOptions> depositsOptions,
        RPCClient client,
        IServiceScopeFactory serviceScopeFactory,
        BitcoinNetworkService bitcoinNetworkService)
    {
        _logger = logger;
        _depositsOptions = depositsOptions.Value;
        _client = client;
        _serviceScopeFactory = serviceScopeFactory;
        _bitcoinNetworkService = bitcoinNetworkService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                var context = scope.ServiceProvider.GetRequiredService<Context>();

                await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead,
                    cancellationToken);

                var latestBlock = await GetLatestBlock(context, cancellationToken);

                var currentBlock = await _client.GetBlockCountAsync(cancellationToken);

                await ScanBlocks(latestBlock, currentBlock, context, cancellationToken);

                await context.Variables
                    .Where(x => x.Key == BitcoinBlock.Key)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Value, currentBlock), cancellationToken);

                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find new deposits");
            }

            await Task.Delay(_depositsOptions.JobInterval, cancellationToken);
        }
    }

    private async Task<int> GetLatestBlock(Context context, CancellationToken cancellationToken)
    {
        var latestBlock = await context.Variables
            .SingleOrDefaultAsync(x => x.Key == BitcoinBlock.Key, cancellationToken);

        if (latestBlock == null)
        {
            var latestBlockCount = await _client.GetBlockCountAsync(cancellationToken);

            var variable = new Variable
            {
                Key = BitcoinBlock.Key,
                Value = latestBlockCount
            };

            await context.Variables.AddAsync(variable, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return variable.Value;
        }

        return latestBlock.Value;
    }

    private async Task ScanBlocks(
        int latestBlock, 
        int currentBlock, 
        Context context, 
        CancellationToken cancellationToken)
    {
        var depositAddress = await context.DepositAddresses.ToArrayAsync(cancellationToken);

        var network = _bitcoinNetworkService.GetNetwork();

        for (var h = latestBlock; h <= currentBlock; h++)
        {
            var block = await _client.GetBlockAsync(h, cancellationToken);

            var txOuts = from t in block.Transactions
                         from o in t.Outputs
                         select new
                         {
                             TxId = t.GetHash().ToString(),
                             Address = o.ScriptPubKey.GetDestinationAddress(network)?.ToString(),
                             Amount = o.Value
                         };

            var deposits = from t in txOuts
                           join d in depositAddress on t.Address equals d.CryptoAddress
                           select new CryptoDeposit
                           {
                               UserId = d.UserId,
                               AddressId = d.Id,
                               Amount = t.Amount.ToDecimal(MoneyUnit.BTC),
                               CurrencyId = d.CurrencyId,
                               CreatedAt = DateTime.UtcNow,
                               TxId = t.TxId,
                               Status = DepositStatus.Created
                           };

            if (deposits.Any())
            {
                await context.CryptoDeposits.AddRangeAsync(deposits, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
