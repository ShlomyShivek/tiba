namespace tiba.rest.services;

public interface IRabbitMqClient
{
    Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, string queueName, int timeoutMs = 30000);
}