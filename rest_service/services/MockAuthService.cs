using System.IdentityModel.Tokens.Jwt;
using Tiba.Rest.Exceptions;

namespace Tiba.Rest.Services;

public class MockAuthService : IAuthService
{
    private int GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub" || x.Type == "userId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        throw new UnauthorizedException("Invalid token: User ID claim not found.");
    }

    public int TryGetUserIdFromAuth(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            throw new UnauthorizedException("Authorization header is missing or invalid.");
        }

        try
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = GetUserIdFromToken(token);
            Console.WriteLine($"User ID extracted from token: {userId}");
            return userId;
        }
        catch (Exception ex)
        {
            // Log the exception if necessary
            Console.WriteLine($"Error getting user ID from token: {ex.Message}");
            throw new UnauthorizedException("Invalid token: Unable to retrieve user ID.");
        }
    }
}