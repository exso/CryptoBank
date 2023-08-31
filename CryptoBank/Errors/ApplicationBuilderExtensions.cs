using CryptoBank.Errors.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoBank.Errors;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
                var exception = exceptionHandlerPathFeature.Error;

                if (exception is ValidationErrorsException validationErrorsException)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Title = "Validation failed",
                        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                        Detail = validationErrorsException.Message,
                        Status = StatusCodes.Status400BadRequest,
                    };

                    problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? context.TraceIdentifier);

                    problemDetails.Extensions["errors"] = validationErrorsException.Errors
                        .Select(x => new ErrorDataWithCode(x.Field, x.Message, x.Code));

                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                }
            });
        });

        return app;
    }
}

internal record ErrorDataWithCode(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("code")] string Code);
