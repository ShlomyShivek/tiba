using Tiba.Rest.Exceptions;

namespace Tiba.Rest.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<UnauthorizedExceptionHandler>();
        return services;
    }
}