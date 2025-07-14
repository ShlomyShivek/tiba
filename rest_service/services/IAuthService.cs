namespace Tiba.Rest.Services;

public interface IAuthService
{
    int TryGetUserIdFromAuth(HttpContext context);
}