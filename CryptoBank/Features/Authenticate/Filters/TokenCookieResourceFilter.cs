using CryptoBank.Features.Authenticate.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.Authenticate.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class TokenCookieResourceFilter : Attribute, IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var features = httpContext.Features.Get<TokenFeature>();

        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<CookieOptions>>().Value;
   
        context.HttpContext.Response.Cookies.Append("refreshToken", features!.Token, options);

        await next();
    }
}
