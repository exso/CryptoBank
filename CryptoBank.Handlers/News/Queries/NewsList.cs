using CryptoBank.Objects.News;
using CryptoBank.Options;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBank.Handlers.News.Queries
{
    public class NewsList
    {
        public record Query : IRequest<Result>;

        public class Result
        {
            public IQueryable<New> Entries { get; set; }
        }

        public class Handler : DbContextBase, IRequestHandler<Query, Result>
        {
            private readonly int _maxCount;
            public Handler(DbContext dbContext, IOptions<NewsOptions> options) : base(dbContext)
            {
                _maxCount = options.Value.MaxCount;
            }

            public Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var entity = DbContext.Set<New>()
                    .AsNoTracking()
                    .OrderByDescending(x => x.Date)
                    .Take(_maxCount)                
                    .AsQueryable();

                return Task.FromResult(new Result { Entries = entity });
            }
        }
    }
}
