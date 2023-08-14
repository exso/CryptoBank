using CryptoBank.Database;
using CryptoBank.Options;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddDbContext<DbContext, Context>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")
    ?? throw new ArgumentNullException("DefaultConnection", "Please provide valid connection string")));

builder.Services.AddMediatR(a => a.RegisterServicesFromAssemblies(new[] {
    typeof(Program).Assembly,
    typeof(CryptoBank.Handlers.News.Queries.NewsList.Handler).Assembly
}));

builder.Services.Configure<NewsOptions>(configuration.GetSection("LatestNews"));

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<Context>();
await context.Database.MigrateAsync();

app.Run();