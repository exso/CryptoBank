using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Services;
using FastEndpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public class RevokeTokenRequest : EndpointWithoutRequest<Unit>
{
    private readonly Context _context;
    private readonly IRefreshTokenCookie _refreshTokenCookie;

    public RevokeTokenRequest(
        Context context,
        IRefreshTokenCookie refreshTokenCookie)
    {
        _context = context;
        _refreshTokenCookie = refreshTokenCookie;
    }

    public override void Configure()
    {
        Post("/revokeToken");
        AllowAnonymous();
    }

    public override async Task<Unit> HandleAsync(CancellationToken cancellationToken)
    {
        var token = _refreshTokenCookie.GetRefreshTokenCookie()
            ?? throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);

        var currentRefreshToken = await _context.RefreshTokens
        .SingleAsync(x => x.Token == token, cancellationToken);

        if (!currentRefreshToken.IsActive)
        {
            throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
        }

        currentRefreshToken.Revoked = DateTime.UtcNow;
        currentRefreshToken.ReasonRevoked = "Revoked token";
        currentRefreshToken.IsRevoked = true;
        currentRefreshToken.IsActive = false;

        _context.RefreshTokens.Update(currentRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
