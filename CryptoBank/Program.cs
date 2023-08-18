using CryptoBank.Database;
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
    // Can be merged if necessary
    .AddOpenBehavior(typeof(LoggingBehavior<,>))
    //.AddOpenBehavior(typeof(MetricsBehavior<,>))
    //.AddOpenBehavior(typeof(TracingBehavior<,>))
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton<Dispatcher>();

builder.Services.AddFastEndpoints();

// Features
builder.AddNews();
builder.AddManagement();

var app = builder.Build();

//Telemetry.Init("Vertical");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//app.MapMetrics();

app.MapFastEndpoints();

app.Run();