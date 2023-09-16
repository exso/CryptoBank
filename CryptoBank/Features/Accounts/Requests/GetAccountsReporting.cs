using CryptoBank.Authorization;
using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Accounts.Models;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Accounts.Errors.Codes.AccountsValidationErrors;

namespace CryptoBank.Features.Accounts.Requests;

public static class GetAccountsReporting
{
    [Authorize(Policy = PolicyNames.AnalystRole)]
    [HttpPost("/accountsReporting")]
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

    public record Request(DateTime StartDate, DateTime EndDate) : IRequest<Response>;

    public record Response(ReportModel[] Entity);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty();

            RuleFor(x => x.EndDate)
                .NotEmpty();
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var startDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

            var report = await _context.Accounts
                .Where(x => x.DateOfOpening >= startDate && x.DateOfOpening <= endDate)
                .GroupBy(x => x.DateOfOpening.Date)
                .Select(x => new
                {
                    Period = x.Key,
                    Count = x.Count()
                })
                .OrderBy(x => x.Period.Date)
                .Select(x => new ReportModel
                {
                    Report = $"{x.Period:dd.MM.yyyy} - {x.Count} accounts"
                })
                .ToArrayAsync(cancellationToken);

            if (!report.Any())
            {
                throw new ValidationErrorsException($"{nameof(report)}", "Data not found", DataNotFound);
            }

            return new Response(report);
        }
    }
}
