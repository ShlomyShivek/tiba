using System.Net;
using Microsoft.AspNetCore.Diagnostics;

namespace Tiba.Rest.Exceptions;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is UnauthorizedException)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await httpContext.Response.WriteAsync(exception.Message, cancellationToken);
            return true;
        }
        return false;
    }
}
