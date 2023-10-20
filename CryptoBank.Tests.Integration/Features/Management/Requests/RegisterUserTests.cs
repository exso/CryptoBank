using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Requests;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Management.Requests;

[Collection(UsersTestsCollection.Name)]
public class RegisterUserTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public RegisterUserTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_register_user()
    {
        // Arrange
        string email = "admin@mail.ru";
        string password = "12345678";
        DateTime dateOfBirth = DateTime.Now.ToUniversalTime();

        var role = new Role { Name = "Administrator", Description = "Администратор" };

        await _fixture.Database.Execute(async x =>
        {
            x.Roles.Add(role);
            await x.SaveChangesAsync();
        });

        var client = _fixture.HttpClient.CreateClient();

        // Act
        var response = (await client.PostAsJsonAsync("/register/user", new
        {
            Email = email,
            Password = password,
            DateOfBirth = dateOfBirth
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userContract = await _fixture.Database.Execute(async x => 
            await x.Users.SingleOrDefaultAsync(u => u.Email == email));

        userContract.Should().NotBeNull();
        userContract!.Email.Should().Be(email);
        userContract.DateOfBirth.Date.Should().Be(dateOfBirth.Date);

        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();
        passwordHasher.VerifyHashedPassword(userContract.Password, password).Should().BeTrue();
    }

    [Fact]
    public async Task Should_return_internal_server_error_if_role_not_found()
    {
        // Arrange
        var client = _fixture.HttpClient.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/register/user", new
        {
            Email = "admin@mail.ru",
            Password = "12345678",
            DateOfBirth = DateTime.Now.ToUniversalTime()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
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

[Collection(UsersTestsCollection.Name)]
public class RegisterValidatorTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    private RegisterUser.RequestValidator? _validator;

    public RegisterValidatorTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_email(string email)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("NotEmptyValidator");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public async Task Should_validate_email_format(string email)
    {
        var result = await _validator.TestValidateAsync(new
            RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("EmailValidator");
    }

    [Fact]
    public async Task Should_validate_email_taken()
    {
        const string email = "test@test.com";

        var existingUser = new User
        {
            Email = email,
            Password = "123",
            DateOfRegistration = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
        };

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(existingUser);
            await x.SaveChangesAsync();
        });

        var result = await _validator.TestValidateAsync(new
            RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("AsyncPredicateValidator");
    }

    [Theory]
    [InlineData("t")]
    [InlineData("ttt")]
    [InlineData("tttt")]
    public async Task Should_validate_email_minimum_length(string email)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request(email, "123456", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_email_maximum_length()
    {
        string email = new('A', 21);

        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request(email, "123456", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_password(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("NotEmptyValidator");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("1234")]
    public async Task Should_validate_password_minimum_length(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_password_maximum_length()
    {
        string password = new('A', 21);

        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("MaximumLengthValidator");
    }

    public async Task InitializeAsync()
    {
        await _fixture.Database.Clear(Create.CancellationToken());
        _scope = _fixture.Factory.Services.CreateAsyncScope();
        _validator = new RegisterUser.RequestValidator(_scope.ServiceProvider.GetRequiredService<Context>());
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }
}