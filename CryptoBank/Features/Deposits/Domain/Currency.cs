namespace CryptoBank.Features.Deposits.Domain;

public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<DepositAddress> DepositAddresses { get; set; } = new HashSet<DepositAddress>();
    public ICollection<Xpub> Xpubs { get; set; } = new HashSet<Xpub>();
}