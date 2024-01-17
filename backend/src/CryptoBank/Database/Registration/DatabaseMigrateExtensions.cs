using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.Registration;

public static class DatabaseMigrateExtensions
{
    public static async Task DatabaseMigrate(this WebApplication app)
    {
        await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<Context>();

        await context.Database.MigrateAsync();
    }
}
