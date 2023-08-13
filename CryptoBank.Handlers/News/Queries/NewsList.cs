using CryptoBank.Objects.News;
using CryptoBank.Options;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBank.Handlers.News.Queries;

public class NewsList
{
    public record Query : IRequest<Result>;

    public class Result
    {
        public IEnumerable<New> Entries { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly DbContext _context;
        private readonly int _maxCount;
        public Handler(DbContext context, IOptions<NewsOptions> options)
        {
            _context = context;
            _maxCount = options.Value.MaxCount;
        }

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = await _context.Set<New>()
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .Take(_maxCount)
                .ToListAsync(cancellationToken);

            return new Result { Entries = entity };
        }
    }
}