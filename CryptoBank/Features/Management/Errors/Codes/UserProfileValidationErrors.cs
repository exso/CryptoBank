namespace CryptoBank.Features.Management.Errors.Codes;

public static class UserProfileValidationErrors
{
    private const string Prefix = "user_profile_validation_";

    public const string UserNotFound = Prefix + "user_not_found";
    public const string IdentifierNotFound = Prefix + "identifier_not_found";
    public const string EmailRequired = Prefix + "email_required";
    public const string PasswordRequired = Prefix + "password_required";
    public const string InvalidСredentials = Prefix + "invalid_credentials";
    public const string MinimumLength = Prefix + "minimum_length";
    public const string MaximumLength = Prefix + "maximum_length";
}
