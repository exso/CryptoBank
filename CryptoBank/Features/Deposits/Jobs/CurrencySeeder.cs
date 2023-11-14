using CryptoBank.Database;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.Deposits.Jobs;

public class CurrencySeeder : IHostedService
{
    private readonly ILogger<CurrencySeeder> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CurrencySeeder(
        IOptions<DepositsOptions> depositsOptions, 
        ILogger<CurrencySeeder> logger, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _depositsOptions = depositsOptions.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<Context>();

        var currencyExist = await context.Currencies.AnyAsync(cancellationToken);

        if (currencyExist)
            return;
       
        var options = _depositsOptions.Currency!;

        var currency = new Currency
        {
            Code = options.Code,
            Name = options.Name
        };

        await context.Currencies.AddAsync(currency, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency code: {currency.Code}, name: {currency.Name} added", currency.Code, currency.Name);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
