using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Pipeline;
using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Validation;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;
using FluentValidation;

using static CryptoBank.Features.Accounts.Errors.Codes.AccountsValidationErrors;
using static CryptoBank.Features.Accounts.Errors.Codes.AccountsLogicConflictErrors;

namespace CryptoBank.Features.Accounts.Requests;

public static class TransferCash
{
    [HttpPost("/transferCash")]
    [Authorize]
    public class Endpoint : Endpoint<Request, HttpStatusCode>
    {
        private readonly Dispatcher _dispatcher;

        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<HttpStatusCode> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            await _dispatcher.Dispatch(request, cancellationToken);

            return HttpStatusCode.OK;
        }
    }

    public record Request(string FromNumber, string ToNumber, decimal Amount) : IRequest<Unit>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.FromNumber).ValidNumber();
            RuleFor(x => x.ToNumber).ValidNumber();

            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var (fromAccount, toAccount) = await CheckAccounts(request, cancellationToken);

            var (fromAmount, toAmount) = await CreateTransaction(fromAccount.Amount, toAccount.Amount, request.Amount);

            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            await UpdateAccount(fromAccount.Number, fromAmount, cancellationToken);

            await UpdateAccount(toAccount.Number, toAmount, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task<(Account fromAccount, Account toAccount)> CheckAccounts(
            Request request, 
            CancellationToken cancellationToken)
        {
            var fromAccount = await _context.Accounts
                .SingleOrDefaultAsync(x => x.Number == request.FromNumber, cancellationToken);

            var toAccount = await _context.Accounts
                .SingleOrDefaultAsync(x => x.Number == request.ToNumber, cancellationToken);

            if (fromAccount is null || toAccount is null)
            {
                throw new ValidationErrorsException(string.Empty, "Accounts not found", AccountsNotFound);
            }

            if (fromAccount.Amount < request.Amount)
            {
                throw new LogicConflictException("Insufficient amount in the account", InsufficientAmountInTheAccount);
            }

            return (fromAccount, toAccount);
        }

        private static Task<(decimal fromAmount, decimal toAmount)> CreateTransaction(
            decimal fromAccountAmount,
            decimal toAccountAmount,
            decimal amount)
        {
            var fromAmount = Decimal.Subtract(fromAccountAmount, amount);

            var toAmount = Decimal.Add(toAccountAmount, amount);

            var transaction = (fromAmount, toAmount);

            return Task.FromResult(transaction);
        }

        private async Task UpdateAccount(string number, decimal amount, CancellationToken cancellationToken)
        {
            await _context.Accounts
                .Where(x => x.Number == number)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Amount, amount), cancellationToken);
        }
    }
}
