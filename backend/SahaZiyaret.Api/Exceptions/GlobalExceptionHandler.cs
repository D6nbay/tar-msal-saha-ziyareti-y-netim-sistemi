using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SahaZiyaret.Api.Exceptions;

/* BusinessRuleExceptionı yakalayıp 400 ve açıklayıcı mesajla döndürür.*/
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 400,
            Title = "İş kuralı ihlali",
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}