using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Accounts.Domain;

public class Account
{
    public string Number { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount {  get; set; }
    public DateTime DateOfOpening { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = new User();
}
