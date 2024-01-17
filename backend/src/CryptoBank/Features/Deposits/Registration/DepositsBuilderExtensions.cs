using CryptoBank.Features.Deposits.Jobs;
using CryptoBank.Features.Deposits.Options;
using CryptoBank.Features.Deposits.Services;

namespace CryptoBank.Features.Deposits.Registration;

public static class DepositsBuilderExtensions
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DepositsOptions>(builder.Configuration.GetSection("Features:Deposits"));

        builder.Services.AddHostedService<CurrencySeeder>();
        builder.Services.AddHostedService<VariableSeeder>();

        //Важно чтобы XpubSeeder выполнялся после CurrencySeeder
        builder.Services.AddHostedService<XpubSeeder>();
        builder.Services.AddTransient<BitcoinNetworkService>();

        return builder;
    }
}
