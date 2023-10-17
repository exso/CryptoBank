using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using CryptoBank.Database;
using CryptoBank.Tests.Integration.Harnesses.Base;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using CryptoBank.Features.Authenticate.Services;

namespace CryptoBank.Tests.Integration.Harnesses;

public class HttpClientHarness<TProgram> : IHarness<TProgram>
    where TProgram : class
{
    private readonly DatabaseHarness<TProgram, Context> _databaseHarness;
    private WebApplicationFactory<TProgram>? _factory;
    private bool _started;

    public HttpClientHarness(DatabaseHarness<TProgram, Context> databaseHarness)
    {
        _databaseHarness = databaseHarness;
    }

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
    }

    public Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken)
    {
        _factory = factory;
        _started = true;

        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _started = false;

        return Task.CompletedTask;
    }

    public HttpClient CreateClient()
    {
        ThrowIfNotStarted();

        return _factory!.CreateClient();
    }

    public async Task<(HttpClient, User user)> CreateAuthenticatedClient(CancellationToken cancellationToken, bool isAnalyst = false)
    {
        ThrowIfNotStarted();

        var user = UserHelper.CreateUser($"{Guid.NewGuid()}@test.com", Guid.NewGuid().ToString(), isAnalyst);

        await _databaseHarness.Execute(async context =>
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        });

        await using var scope = _factory!.Services.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jwt = tokenService.GetAccessToken(user);

        var client = _factory!.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        return (client, user);
    }

    public async Task<(HttpClient, User user)> CreateWronglyAuthenticatedClient(CancellationToken cancellationToken)
    {
        ThrowIfNotStarted();

        var user = new User
        {
            Email = $"{Guid.NewGuid()}@test.com",
            Password = Guid.NewGuid().ToString(),
            DateOfBirth = DateTime.UtcNow,
            DateOfRegistration = DateTime.UtcNow,
        };
        await _databaseHarness.Execute(async context =>
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        });

        // TODO: use config
        var key = "invalidKeyinvalidKeyinvalidKeyinvalidKey"u8.ToArray();
        var token = new JwtSecurityToken(
            issuer: "crypto-bank",
            audience: "crypto-bank",
            claims: new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user!.Id.ToString()),
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        var client = _factory!.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        return (client, user);
    }

    private void ThrowIfNotStarted()
    {
        if (!_started)
        {
            throw new InvalidOperationException($"HTTP client harness is not started. Call {nameof(Start)} first.");
        }
    }
}
