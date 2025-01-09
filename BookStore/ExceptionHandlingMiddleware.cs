using Infrastructure.Exceptions;
using System.Net;
using System.Security.Authentication;

namespace BookStore;
public class ExceptionsHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsHandlingMiddleware> _logger;

    public ExceptionsHandlingMiddleware(RequestDelegate next, ILogger<ExceptionsHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            if (!context.Response.HasStarted)
                await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    public Task HandleException(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;

        var code = HttpStatusCode.InternalServerError;
        switch (ex)
        {
            case HttpRequestException:
                code = HttpStatusCode.BadRequest;
                break;
            case AccessViolationException:
                code = HttpStatusCode.Unauthorized;
                break;
            case AlreadyExistsException:
                code = HttpStatusCode.Conflict;
                break;
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                break;
            case InvalidCredentialException:
                code = HttpStatusCode.Forbidden;
                break;
            case BadHttpRequestException:
                code = HttpStatusCode.BadRequest; 
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/text";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(ex.Message);
    }
}
