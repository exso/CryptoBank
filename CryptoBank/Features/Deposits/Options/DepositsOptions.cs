using CryptoBank.Features.Deposits.Domain;

namespace CryptoBank.Features.Deposits.Options;

public class DepositsOptions
{
    public BitcoinNetwork BitcoinNetwork { get; set; }
    public Currency? Currency { get; set; }
}

public enum BitcoinNetwork
{
    MainNet,
    TestNet,
    RegTest
}
