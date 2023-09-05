using CryptoBank.Database;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Features.Authenticate.Requests;

public static class RefreshTokenRequest
{
    [HttpPost("/refreshToken")]
    [AllowAnonymous]
    public class Endpoint : Endpoint<Request, Response>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<Response> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await _dispatcher.Dispatch(request, cancellationToken);
    }

    public record Request(string AccessToken) : IRequest<Response>;
    public record Response();

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly ITokenService _tokenService;

        public RequestHandler(Context context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var refreshToken = _tokenService.GetRefreshTokenCookie();

            var user = await _context.Users
                .Include(x => x.RefreshTokens)
                .ThenInclude(x => x.Token)
                .SingleAsync(x => x.RefreshTokens.Any(x => x.Token == refreshToken), cancellationToken);

            return new Response();
        }
    }
}
