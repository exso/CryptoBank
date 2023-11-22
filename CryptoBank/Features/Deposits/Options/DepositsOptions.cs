using CryptoBank.Features.Deposits.Domain;

namespace CryptoBank.Features.Deposits.Options;

public class DepositsOptions
{
    public BitcoinNetwork BitcoinNetwork { get; set; }
    public Currency? Currency { get; set; }
    public TimeSpan JobInterval { get; set; }
    public BitcoinNetworkCredential? BitcoinNetworkCredential { get; set; }
}

public enum BitcoinNetwork
{
    MainNet,
    TestNet,
    RegTest
}

public class BitcoinNetworkCredential
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
