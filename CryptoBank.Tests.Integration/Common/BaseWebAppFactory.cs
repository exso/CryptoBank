using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CryptoBank.Tests.Integration.Common;

public class BaseWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    "ConnectionStrings:DefaultConnection",
                    "User ID=postgres;Password=12345678;Server=localhost;Port=5432;Database=cryptoBankTestDb;Integrated Security=true;Pooling=true;"
                },
            });
        });
    }
}