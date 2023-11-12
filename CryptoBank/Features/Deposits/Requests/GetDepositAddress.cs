using CryptoBank.Common.Services;
using CryptoBank.Database;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CryptoBank.Features.Deposits.Requests;

public static class GetDepositAddress
{
    [HttpGet("/getDepositAddress")]
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

            var response = await _dispatcher.Dispatch(new Request(userId, ""), cancellationToken);

            return response;
        }
    }

    public record Request(int UserId, string CurrencyCode) : IRequest<Response>;

    public record Response(string CryptoAddress);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
