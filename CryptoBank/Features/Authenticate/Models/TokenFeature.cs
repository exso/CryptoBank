namespace CryptoBank.Features.Authenticate.Models;

public class TokenFeature
{
    public string Token { get; set; }   
    public TokenFeature(string token)
    {
        Token = token;
    }
}
