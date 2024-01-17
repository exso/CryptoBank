namespace CryptoBank.Features.Authenticate.Options;

public class AuthenticateOptions
{
    public JwtOptions Jwt { get; set; } = new JwtOptions();
    public RefreshTokenOptions RefreshToken { get; set; } = new RefreshTokenOptions();

    public class JwtOptions
    {
        public string SigningKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public TimeSpan Expiration { get; set; }
    }
}