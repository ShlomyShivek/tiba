namespace Tiba.Rest.Services;

/// <summary>
/// Service interface for handling authentication operations.
/// Provides methods for extracting user information from HTTP context.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Attempts to extract the user ID from the authentication context.
    /// </summary>
    /// <param name="context">The HTTP context containing authentication information.</param>
    /// <returns>The user ID extracted from the authentication context.</returns>
    int TryGetUserIdFromAuth(HttpContext context);
}