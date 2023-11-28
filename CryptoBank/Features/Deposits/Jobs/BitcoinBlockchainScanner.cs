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

                var network = _bitcoinNetworkService.GetNetwork();

                await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead,
                    cancellationToken);

                var depositAddress = await context.DepositAddresses.ToArrayAsync(cancellationToken);

                await SaveLatestBlock(context, cancellationToken);

                var latestBlock = await GetLatestBlock(context, cancellationToken);

                var currentBlock = await _client.GetBlockCountAsync(cancellationToken);

                for (var h = latestBlock; h <= currentBlock; h++)
                {
                    var block = await _client.GetBlockAsync(h, cancellationToken);

                    var txOuts = from t in block.Transactions.SelectMany(t => t.Outputs)
                                 select new
                                 {
                                     TxId = string.Empty,
                                     Address = t.ScriptPubKey.GetDestinationAddress(network)?.ToString(),
                                     t.Value
                                 };

                    var deposits = from t in txOuts
                                   join d in depositAddress on t.Address equals d.CryptoAddress
                                   select new CryptoDeposit
                                   {
                                       UserId = d.UserId,
                                       AddressId = d.Id,
                                       Amount = t.Value.ToDecimal(MoneyUnit.BTC),
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
                
                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find new deposits");
            }

            await Task.Delay(_depositsOptions.JobInterval, cancellationToken);
        }
    }

    private async Task SaveLatestBlock(Context context, CancellationToken cancellationToken)
    {
        var latestBlock = await _client.GetBlockCountAsync(cancellationToken);

        var variable = new Variable
        {
            Key = BitcoinBlock.Key,
            Value = latestBlock
        };

        await context.Variables.AddAsync(variable, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<int> GetLatestBlock(Context context, CancellationToken cancellationToken)
    {
        var latestBlock = await context.Variables
            .SingleAsync(x => x.Key == BitcoinBlock.Key, cancellationToken);

        return latestBlock.Value;
    }
}
