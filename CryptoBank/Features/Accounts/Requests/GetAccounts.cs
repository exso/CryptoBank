using CryptoBank.Common.Services;
using CryptoBank.Database;
using CryptoBank.Features.Accounts.Models;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Features.Accounts.Requests;

public static class GetAccounts
{
    [HttpGet("/getAccounts")]
    [Authorize]
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        private readonly Dispatcher _dispatcher;
        private readonly UserIdentifierService _userIdentifierService;

        public Endpoint(Dispatcher dispatcher, UserIdentifierService userIdentifierService)
        {
            _dispatcher = dispatcher;
            _userIdentifierService = userIdentifierService;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken)
        {
            var userId = _userIdentifierService.GetUserIdentifier();

            var response = await _dispatcher.Dispatch(new Request(userId), cancellationToken);

            return response;
        }
    }

    public record Request(int UserId) : IRequest<Response>;

    public record Response(AccountsModel[] Accounts);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var accounts = await _context.Accounts
                .Where(x => x.UserId == request.UserId)
                .Select(x => new AccountsModel
                {
                    Number = x.Number,
                    Currency = x.Currency,
                    Amount = x.Amount,
                    DateOfOpening = x.DateOfOpening,
                    UserEmail = x.User.Email
                })
                .ToArrayAsync(cancellationToken);

            return new Response(accounts);
        }
    }
}