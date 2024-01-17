using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Deposits.Domain;

namespace CryptoBank.Features.Management.Domain;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfRegistration { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public ICollection<UserToken> UserTokens { get; set; } = new HashSet<UserToken>();
    public ICollection<Account> UserAccounts { get; set; } = new HashSet<Account>();
    public ICollection<DepositAddress> DepositAddresses { get; set; } = new HashSet<DepositAddress>();
}
