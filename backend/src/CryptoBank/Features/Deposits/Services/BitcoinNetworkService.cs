using CryptoBank.Features.Deposits.Options;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace CryptoBank.Features.Deposits.Services;

public class BitcoinNetworkService
{
    private readonly DepositsOptions _depositsOptions;

    public BitcoinNetworkService(IOptions<DepositsOptions> depositsOptions)
    {
        _depositsOptions = depositsOptions.Value;
    }

    public Network GetNetwork()
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
