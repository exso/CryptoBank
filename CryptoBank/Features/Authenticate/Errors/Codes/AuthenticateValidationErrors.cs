namespace CryptoBank.Features.Authenticate.Errors.Codes;

public static class AuthenticateValidationErrors
{
    private const string Prefix = "authenticate_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string InvalidСredentials = Prefix + "invalid_credentials";
    public const string PasswordRequired = Prefix + "password_required";
}
