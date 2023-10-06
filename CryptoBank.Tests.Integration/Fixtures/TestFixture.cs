using Microsoft.AspNetCore.Mvc.Testing;
using CryptoBank.Database;
using CryptoBank.Tests.Integration.Harnesses;
using CryptoBank.Tests.Integration.Harnesses.Base;
using CryptoBank.Tests.Integration.Helpers;

namespace CryptoBank.Tests.Integration.Fixtures;

public class TestFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    public TestFixture()
    {
        Database = new();
        HttpClient = new(Database);

        _factory = new WebApplicationFactory<Program>()
            .AddHarness(Database)
            .AddHarness(HttpClient);
    }

    public WebApplicationFactory<Program> Factory => _factory;
    public DatabaseHarness<Program, Context> Database { get; }
    public HttpClientHarness<Program> HttpClient { get; }

    public async Task InitializeAsync()
    {
        await Database.Start(_factory, Create.CancellationToken());
        await HttpClient.Start(_factory, Create.CancellationToken());

        var _ = _factory.Server;
    }

    public async Task DisposeAsync()
    {
        await HttpClient.Stop(Create.CancellationToken());
        await Database.Stop(Create.CancellationToken());
    }
}
