using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tiba.Rest.Exceptions;
using Tiba.Rest.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tiba.Rest.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
        services.AddSingleton<ITodoService, TodoService>();
        services.AddSingleton<IAuthService, MockAuthService>();

        return services;

    }

    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        //TOOD: change to actual health checks
        services.AddHealthChecks()
            .AddCheck("api", () => HealthCheckResult.Healthy("API is running"))
            .AddCheck("database", () => HealthCheckResult.Healthy("Database connection is healthy"));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                //TODO: this is a mock implementation, replace with actual JWT validation logic
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireExpirationTime = false,
                    SignatureValidator = (token, parameters) =>
                    {
                        // Custom signature validation logic can be added here if needed
                        return new JsonWebToken(token);
                    },
                    ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance for token expiration timing
                };
            });
        return services;
    }

    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<UnauthorizedExceptionHandler>();
        return services;
    }
}