using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Domain;

public class UserToken
{
    public int Id { get; set; } 
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Revoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => Expires < DateTime.UtcNow;
    public bool IsRevoked => Revoked is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    public User User { get; set; } = new User();
}
