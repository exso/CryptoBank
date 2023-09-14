using CryptoBank.Authorization;
using CryptoBank.Authorization.Requirements;
using CryptoBank.Common.Registration;
using CryptoBank.Database;
using CryptoBank.Database.Registration;
using CryptoBank.Errors;
using CryptoBank.Features.Accounts.Registration;
using CryptoBank.Features.Authenticate.Registration;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Registration;
using CryptoBank.Features.News.Registration;
using CryptoBank.Pipeline;
using CryptoBank.Pipeline.Behaviors;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(LoggingBehavior<,>))
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton<Dispatcher>();

builder.Services.AddFastEndpoints();

//Common
builder.AddCommon();

// Features
builder.AddNews();
builder.AddManagement();
builder.AddAuthenticate();
builder.AddAccounts();

builder.Services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdministratorRole, policy => policy.AddRequirements(new RoleRequirement(Roles.Administrator)));
    options.AddPolicy(PolicyNames.AnalystRole, policy => policy.AddRequirements(new RoleRequirement(Roles.Analyst)));
    options.AddPolicy(PolicyNames.UserRole, policy => policy.AddRequirements(new RoleRequirement(Roles.User)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

await app.DatabaseMigrate();

await app.SeedDatabase();

app.MapProblemDetailsWithLogicConflicts();

app.UseAuthentication();
app.UseAuthorization();

app.MapFastEndpoints();

app.Run();