namespace CryptoBank.Features.Authenticate.Errors.Codes;

public static class AuthenticateValidationErrors
{
    private const string Prefix = "authenticate_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string EmailNotFound = Prefix + "email_not_found";
    public const string EmailInvalid = Prefix + "email_invalid";
    public const string PasswordRequired = Prefix + "password_required";
    public const string PasswordInvalid = Prefix + "password_invalid";
}
