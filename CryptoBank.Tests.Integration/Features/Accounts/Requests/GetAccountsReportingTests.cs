using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using CryptoBank.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Accounts.Requests;
using FluentValidation.TestHelper;
using System.Text.Json;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

public class GetAccountsReportingTests : IClassFixture<BaseWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly BaseWebAppFactory<Program> _factory;
    private Context _context;
    private AsyncServiceScope _scope;
    private Argon2IdPasswordHasher _passwordHasher;

    public GetAccountsReportingTests(BaseWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_get_accounts_reporting()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");
        user.UserRoles.Add(new UserRole { Role = new() { Name = "Analyst", Description = "Аналитик" } });

        var (account1, account2) = AccountsHelper.CreateAccounts(user, "BTC", 100);
        await _context.SaveChangesAsync();

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = (await client.PostAsJsonAsync("/accountsReporting", new
        {
            StartDate = DateTime.Now.AddDays(-2).ToUniversalTime(),
            EndDate = DateTime.Now.ToUniversalTime()
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accountContract1 = await _context.Accounts.SingleOrDefaultAsync(x => x.Number == account1.Number);
        AccountsHelper.AssertAccount(accountContract1!, account1);

        var accountContract2 = await _context.Accounts.SingleOrDefaultAsync(x => x.Number == account2.Number);
        AccountsHelper.AssertAccount(accountContract2!, account2);

        var content = await response.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<GetAccountsReporting.Response>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        contract!.Entity.Should().NotBeNull();
        contract.Entity.Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_wrong_role()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");

        await _context.SaveChangesAsync();

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsJsonAsync("/accountsReporting", new
        {
            StartDate = "2023-09-10",
            EndDate = "2023-09-16"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_invalid_token()
    {
        // Arrange
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwia";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsJsonAsync("/accountsReporting", new
        {
            StartDate = "2023-09-10",
            EndDate = "2023-09-16"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync()
    {
        var _ = _factory.Server;
        _scope = _factory.Services.CreateAsyncScope();
        _context = _scope.ServiceProvider.GetRequiredService<Context>();
        _passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _context.Accounts.RemoveRange(_context.Accounts);
        _context.UserRoles.RemoveRange(_context.UserRoles);
        _context.Roles.RemoveRange(_context.Roles);
        _context.Users.RemoveRange(_context.Users);

        await _context.DisposeAsync();
        await _scope.DisposeAsync();
    }
}

public class GetAccountsReportingValidatorTests
{
    private readonly GetAccountsReporting.RequestValidator _validator = new();

    [Fact]
    public void Should_validate_correct_request()
    {
        var startDate = DateTime.Now.ToUniversalTime();
        var endDate = DateTime.Now.ToUniversalTime().AddDays(1);

        var result = _validator.TestValidate(new GetAccountsReporting.Request(startDate, endDate));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_validate_start_date_less_than_end_date()
    {
        var startDate = DateTime.Now.ToUniversalTime().AddDays(1);
        var endDate = DateTime.Now.ToUniversalTime();

        var result = await _validator.TestValidateAsync(new GetAccountsReporting.Request(startDate, endDate));
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorCode("accounts_validation_start_date_must_be_before_end_date");
    }

    [Fact]
    public async Task Should_require_start_date()
    {
        var startDate = DateTime.MinValue;
        var endDate = DateTime.Now.ToUniversalTime();

        var result = await _validator.TestValidateAsync(new GetAccountsReporting.Request(startDate, endDate));
        result.ShouldHaveValidationErrorFor(x => x.StartDate).WithErrorCode("accounts_validation_date_not_empty");
    }

    [Fact]
    public async Task Should_require_end_date()
    {
        var startDate = DateTime.Now.ToUniversalTime();
        var endDate = DateTime.MinValue;

        var result = await _validator.TestValidateAsync(new GetAccountsReporting.Request(startDate, endDate));
        result.ShouldHaveValidationErrorFor(x => x.EndDate).WithErrorCode("accounts_validation_date_not_empty");
    }
}
