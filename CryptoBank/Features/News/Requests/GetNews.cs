using CryptoBank.Authorization;
using CryptoBank.Database;
using CryptoBank.Features.News.Models;
using CryptoBank.Features.News.Options;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.News.Requests;

public static class GetNews
{
    [Authorize(Policy = PolicyNames.UserRole)]
    [HttpGet("/news")]
    public class Endpoint : Endpoint<Request, NewModel[]>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<NewModel[]> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
             await _dispatcher.Dispatch(request, cancellationToken);
    }

    public record Request() : IRequest<NewModel[]>;

    public class RequestHandler : IRequestHandler<Request, NewModel[]>
    {
        private readonly Context _context;
        private readonly int _maxCount;

        public RequestHandler(Context context, IOptions<NewsOptions> options)
        {
            _context = context;
            _maxCount = options.Value.MaxCount;
        }

        public async Task<NewModel[]> Handle(Request request, CancellationToken cancellationToken) =>
            await _context.News
                .OrderByDescending(x => x.Date)
                .Take(_maxCount)
                .Select(x => new NewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Date = x.Date,
                    Author = x.Author,
                    Description = x.Description,                
                })
                .ToArrayAsync(cancellationToken);
    }
}
