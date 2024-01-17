namespace CryptoBank.Features.Accounts.Models;

public class AccountsModel
{
    public string Number { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateOfOpening { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}
