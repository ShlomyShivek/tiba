namespace Tiba.Rest.Services;

public interface IRabbitMqClient
{
    Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, string queueName, int timeoutMs = 240000);
}