namespace CryptoBank.Features.Authenticate.Options;

public class RefreshTokenOptions
{
    public TimeSpan Expiration { get; set; }
    public TimeSpan ArchiveExpiration { get; set; }
    public TimeSpan JobInterval { get; set; }
}
