using CryptoBank.Common.Passwords;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Requests;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Management.Requests;

[Collection(UsersTestsCollection.Name)]
public class GetUserProfileTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public GetUserProfileTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_get_user_profile()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = new User
        {
            Email = "example@example.com",
            Password = passwordHasher.HashPassword("qwerty123456A!"),
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            DateOfRegistration = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
            {
                new()
                {
                    Role = new Role
                    {
                        Name = "User", Description = "Обычный пользователь"
                    }
                }
            }
        };

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetFromJsonAsync<GetUserProfile.Response>("/profile");
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
