using CryptoBank.Features.Management.Options;
using CryptoBank.Features.Management.Services;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Features.Management.Registration;

public static class ManagmentBuilderExtensions
{
    public static WebApplicationBuilder AddManagement(this WebApplicationBuilder builder)
    {
        // Fake DbContext to satisfy service dependencies
        builder.Services.AddScoped<DbContext>();

        builder.Services.Configure<ManagmentOptions>(builder.Configuration.GetSection("Features:Management"));

        builder.Services.AddTransient<RoleSeeder>();

        return builder;
    }

    public static async Task SeedDatabase(this WebApplication app)
    {
        await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

        var seeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();

        await seeder!.Seed();
    }
}
