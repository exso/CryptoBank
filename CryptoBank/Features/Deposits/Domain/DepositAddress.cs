using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Deposits.Domain;

public class DepositAddress
{
    public int Id { get; set; }
    public int CurrencyId { get; set; }
    public int UserId { get; set; } 
    public int XpubId { get; set; }  
    public int DerivationIndex { get; set; }
    public string CryptoAddress { get; set; } = string.Empty;

    public Currency? Currency { get; set; }
    public User? User { get; set; }
    public Xpub? Xpub { get; set; }
}