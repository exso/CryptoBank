using CryptoBank.Features.News.Options;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Features.News.Registration;

public static class NewsBuilderExtensions
{
    public static WebApplicationBuilder AddNews(this WebApplicationBuilder builder)
    {
        // Fake DbContext to satisfy service dependencies
        builder.Services.AddScoped<DbContext>();

        builder.Services.Configure<NewsOptions>(builder.Configuration.GetSection("Features:News"));

        return builder;
    }
}
