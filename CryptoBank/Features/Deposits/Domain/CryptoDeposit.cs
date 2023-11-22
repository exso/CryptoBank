using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Deposits.Domain;

public class CryptoDeposit
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TxId { get; set; } = string.Empty;
    public uint Confirmations { get; set; }
    public DepositStatus Status { get; set; }

    public User? User { get; set; }
    public DepositAddress? Address { get; set; }
    public Currency? Currency { get; set; }
}

public enum DepositStatus
{
    Created,
    Confirmed,
}