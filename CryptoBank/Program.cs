using CryptoBank.Database;
using CryptoBank.Database.Registration;
using CryptoBank.Features.Authenticate.Registration;
using CryptoBank.Features.Management.Registration;
using CryptoBank.Features.News.Registration;
using CryptoBank.Pipeline;
using CryptoBank.Pipeline.Behaviors;
using FastEndpoints;
using FluentValidation;
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

// Features
builder.AddNews();
builder.AddManagement();
builder.AddAuthenticate();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

await app.DatabaseMigrate();

await app.SeedDatabase();

app.UseAuthentication();
app.UseAuthorization();

app.MapFastEndpoints();

app.Run();