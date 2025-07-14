using System;

namespace Tiba.Rest.Extentions;

public static class EndpointExtensions
{
    public static WebApplication AddGenericExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new { message = "An internal server error occurred." };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            });
        });
        return app;
    }
}
