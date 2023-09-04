using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Domain;

public class RefreshToken
{
    public int Id { get; set; } 
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; } 
    public bool IsExpired { get; set; }
    public bool IsRevoked {  get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; } = new User();
}
