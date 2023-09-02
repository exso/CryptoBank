namespace CryptoBank.Features.Authenticate.Options;

public class AuthenticateOptions
{
    public JwtOptions Jwt { get; set; }

    public class JwtOptions
    {
        public string SigningKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public TimeSpan Expiration { get; set; }
    }
}