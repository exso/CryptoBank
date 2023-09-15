using CryptoBank.Errors.Exceptions;
using System.Security.Claims;

using static CryptoBank.Errors.Codes.ValidationErrorsCode;

namespace CryptoBank.Common.Services;

public class UserIdentifierService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserIdentifierService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserIdentifier()
    {
        var identifier = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(identifier, out int userId))
        {
            return userId;
        }

        throw new ValidationErrorsException($"{nameof(identifier)}", "Identifier not found", IdentifierNotFound);
    }
}