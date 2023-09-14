namespace CryptoBank.Features.Management.Errors.Codes;

public static class UserProfileValidationErrors
{
    private const string Prefix = "user_profile_validation_";

    public const string UserNotFound = Prefix + "user_not_found";
    public const string IdentifierNotFound = Prefix + "identifier_not_found";
}
