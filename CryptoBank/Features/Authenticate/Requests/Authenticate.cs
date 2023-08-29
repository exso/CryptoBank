using CryptoBank.Database;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Features.Authenticate.Requests;

public static class Authenticate
{
    [HttpPost("/authenticate")]
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

    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            //TODO проверить пользователя и пароль
            RuleFor(x => x.Email)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20)
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly IAccessTokenService _accessTokenService;

        public RequestHandler(Context context, IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            var token = _accessTokenService.GetAccessToken(user!);

            return new Response(token);
        }
    }
}
