using CryptoBank.Features.Deposits.Options;
using Microsoft.Extensions.Options;
using NBitcoin.RPC;

namespace CryptoBank.Features.Deposits.Jobs;

public class BitcoinBlockchainScanner : BackgroundService
{
    private readonly ILogger<BitcoinBlockchainScanner> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly RPCClient _client;

    public BitcoinBlockchainScanner(
        ILogger<BitcoinBlockchainScanner> logger,
        IOptions<DepositsOptions> depositsOptions,
        RPCClient client)
    {
        _logger = logger;
        _depositsOptions = depositsOptions.Value;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var blockchainInfo = await _client.GetBlockchainInfoAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find new deposits");
            }

            await Task.Delay(_depositsOptions.JobInterval, stoppingToken);
        }
    }
}
