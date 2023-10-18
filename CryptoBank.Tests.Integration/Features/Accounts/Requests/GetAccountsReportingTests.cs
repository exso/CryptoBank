using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using CryptoBank.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using CryptoBank.Features.Accounts.Requests;
using FluentValidation.TestHelper;
using System.Text.Json;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

[Collection(AccountsTestsCollection.Name)]
public class GetAccountsReportingTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public GetAccountsReportingTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_get_accounts_reporting()
    {
        // Arrange
        var userRole = new UserRole() { Role = new Role { Name = "Analyst", Description = "Аналитик" } };

        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken(), userRole);

        var (account1, account2) = AccountsHelper.CreateAccounts(user.Id, "BTC", 100);

        await _fixture.Database.Execute(async x =>
        {
            x.Accounts.AddRange(account1, account2);
            await x.SaveChangesAsync();
        });

        // Act
        var response = (await client.PostAsJsonAsync("/accountsReporting", new
        {
            StartDate = DateTime.Now.AddDays(-2).ToUniversalTime(),
            EndDate = DateTime.Now.ToUniversalTime()
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accountContract1 = await _fixture.Database.Execute(async x =>
            await x.Accounts.SingleOrDefaultAsync(u => u.Number == account1.Number));

        AccountsHelper.AssertAccount(accountContract1!, account1);

        var accountContract2 = await _fixture.Database.Execute(async x =>
            await x.Accounts.SingleOrDefaultAsync(u => u.Number == account2.Number));

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
    public async Task Should_return_forbidden_if_wrong_role()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

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
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsJsonAsync("/accountsReporting", new
        {
            StartDate = "2023-09-10",
            EndDate = "2023-09-16"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async Task InitializeAsync()
    {
        await _fixture.Database.Clear(Create.CancellationToken());

        _scope = _fixture.Factory.Services.CreateAsyncScope();
    }

    public async Task DisposeAsync()
    {
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
