using CryptoBank.Features.Deposits.Jobs;
using CryptoBank.Features.Deposits.Options;
using CryptoBank.Features.Deposits.Services;
using Microsoft.Extensions.Options;
using NBitcoin.RPC;
using System.Net;

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

        builder.Services.AddSingleton(p =>
        {
            var network = p.GetRequiredService<BitcoinNetworkService>()
                .GetNetwork();

            var options = p.GetRequiredService<IOptions<DepositsOptions>>().Value.BitcoinNetworkCredential!;

            var credentials = new NetworkCredential(options.UserName, options.Password);
            var bitcoindUri = new Uri(options.Url);

            return new RPCClient(credentials, bitcoindUri, network);
        });

        builder.Services.AddHostedService<BitcoinBlockchainScanner>();

        return builder;
    }
}
