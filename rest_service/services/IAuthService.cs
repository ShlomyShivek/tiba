namespace tiba.rest.services;

public interface IAuthService
{
    int TryGetUserIdFromAuth(HttpContext context);
}