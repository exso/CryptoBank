using CryptoBank.Database;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.Deposits.Jobs;

public class VariableSeeder : IHostedService
{
    private readonly ILogger<VariableSeeder> _logger;
    private readonly DepositsOptions _depositsOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VariableSeeder(
        IOptions<DepositsOptions> depositsOptions,
        ILogger<VariableSeeder> logger,
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

        var variableExist = await context.Variables.AnyAsync(cancellationToken);

        if (variableExist)
            return;

        var variable = new Variable
        {
            Key = _depositsOptions.DerivationIndex,
            Value = 1
        };

        await context.Variables.AddAsync(variable, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variable key: {variable.Key}, value: {variable.Value} added", variable.Key, variable.Value);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
