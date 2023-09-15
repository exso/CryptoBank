using CryptoBank.Features.Accounts.Options;
using CryptoBank.Features.Accounts.Services;

namespace CryptoBank.Features.Accounts.Registration;

public static class AccountsBuilderExtensions
{
    public static WebApplicationBuilder AddAccounts(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AccountsOptions>(builder.Configuration.GetSection("Features:Accounts"));

        builder.Services.AddTransient<UserIdentifierService>();

        return builder;
    }
}
