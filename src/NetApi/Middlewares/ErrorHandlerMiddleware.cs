using Microsoft.AspNetCore.Diagnostics;
using NetApi.Application.Common.Exceptions;
using NetApi.Models;

namespace NetApi.Middlewares;

public class ErrorHandlerMiddleware(ILogger logger) : IExceptionHandler
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Handles exceptions that occur during the request processing.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new Result<object> {
            Message = "An error occurred while processing your request.",
            Success = false,
            Data = null,
        };

        if (exception is UnauthorizedAccessException || exception is UnauthorizedException) {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            problemDetails.Message = exception.Message ?? "Unauthorized access";
            problemDetails.Errors = new Dictionary<string, List<string>> { { "user", ["Unauthorized"] } };
        } else if (exception is BadHttpRequestException) {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            problemDetails.Message = "Bad request: Invalid input or missing parameters.";
        } else if (exception is NotFoundException) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            problemDetails.Message = exception.Message ?? "Resource not found.";
        } else {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseErrorHandlerMiddleware(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(opt => {
            opt.Run(async ctx => {
                var exceptionHandlerPathFeature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature?.Error == null) return;

                var logger = app.ApplicationServices.GetRequiredService<ILogger<ErrorHandlerMiddleware>>();
                var middleware = new ErrorHandlerMiddleware(logger);
                await middleware.TryHandleAsync(ctx, exceptionHandlerPathFeature.Error, CancellationToken.None);
            });
        });
    }
}