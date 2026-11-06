using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SahaZiyaret.Api.Exceptions;

/* Doğrulama ve iş kuralı hatalarını yakalayıp 400 ile döndürür.*/
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1),
                    g => g.Select(e => e.ErrorMessage).ToArray());
            // Alan adının ilk harfini küçült

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
            {
                Status = 400,
                Title = "Doğrulama hatası"
            }, cancellationToken);
            return true;
        }

        // Kendi iş kuralı hatalarım
        if (exception is BusinessRuleException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "İş kuralı ihlali",
                Detail = exception.Message
            }, cancellationToken);
            return true;
        }

        //Başka hata
        return false;
    }
}