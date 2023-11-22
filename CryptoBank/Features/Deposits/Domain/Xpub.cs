namespace CryptoBank.Features.Deposits.Domain;

public class Xpub
{
    public int Id { get; set; }
    public int CurrencyId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Currency? Currency { get; set; }
    public ICollection<DepositAddress> DepositAddresses { get; set; } = new HashSet<DepositAddress>();
}
