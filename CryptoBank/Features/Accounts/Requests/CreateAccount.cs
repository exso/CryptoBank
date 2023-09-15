using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Accounts.Options;
using CryptoBank.Features.Accounts.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

using static CryptoBank.Features.Accounts.Errors.Codes.AccountsLogicConflictErrors;

namespace CryptoBank.Features.Accounts.Requests;

public static class CreateAccount
{
    [HttpPost("/createAccount")]
    [Authorize]
    public class Endpoint : Endpoint<Request, HttpStatusCode>
    {
        private readonly Dispatcher _dispatcher;
        private readonly UserIdentifierService _userIdentifierService;

        public Endpoint(Dispatcher dispatcher, UserIdentifierService userIdentifierService)
        {
            _dispatcher = dispatcher;
            _userIdentifierService = userIdentifierService;
        }

        public override async Task<HttpStatusCode> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var userId = _userIdentifierService.GetUserIdentifier();

            Request current = request with { UserId = userId };

            await _dispatcher.Dispatch(current, cancellationToken);

            return HttpStatusCode.OK;
        }
    }

    public record Request(string Currency, decimal Amount, int UserId) : IRequest<Unit>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Currency)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(3);

            RuleFor(x => x.Amount)
                .NotEmpty();
        }
    }

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;
        private readonly AccountsOptions _accountsOptions;

        public RequestHandler(Context context, IOptions<AccountsOptions> accountsOptions)
        {
            _context = context;
            _accountsOptions = accountsOptions.Value;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserAccounts)
                .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new ValidationErrorsException($"{nameof(user)}", "User not found", UserNotFound);
            }

            var accountsCount = user.UserAccounts.Count(x => x.UserId == user.Id);

            if (accountsCount >= _accountsOptions.AllowedNumberOfAccounts)
            {
                throw new LogicConflictException("Allowed number of accounts limit", AllowedNumberOfAccountsLimit);
            }
            
            var number = Guid.NewGuid().ToString();

            user.UserAccounts.Add(new Account
            {
                Number = number,
                Currency = request.Currency,
                Amount = request.Amount,
                DateOfOpening = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
