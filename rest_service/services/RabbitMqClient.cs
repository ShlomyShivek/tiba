using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Tiba.Rest.Services;

public class RabbitMqClient : IRabbitMqClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

    public RabbitMqClient()
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
            RequestedConnectionTimeout = TimeSpan.FromSeconds(240)
        };
        

        _connection = factory.CreateConnection("rest_service_client");
        _channel = _connection.CreateModel();

        
        //using direct reply-to pattern https://www.rabbitmq.com/docs/direct-reply-to
        _replyQueueName = "amq.rabbitmq.reply-to";
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            Console.WriteLine($"Received response for correlation ID: {ea.BasicProperties.CorrelationId}");
            if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;

            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        _channel.BasicConsume(consumer: consumer, queue: _replyQueueName, autoAck: true);
    }

    public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, string queueName, int timeoutMs = 240000)
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper[correlationId] = tcs;

        var requestJson = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(requestJson);

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        Console.WriteLine($"Sending request to {queueName} with correlation ID: {correlationId}, ReplyTo: {_replyQueueName}, timeout: {timeoutMs}ms");

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);

        using var cts = new CancellationTokenSource(timeoutMs);
        cts.Token.Register(() => _callbackMapper.TryRemove(correlationId, out _));

        try
        {
            var responseJson = await tcs.Task.WaitAsync(cts.Token);
            return JsonSerializer.Deserialize<TResponse>(responseJson) ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (OperationCanceledException)
        {
            _callbackMapper.TryRemove(correlationId, out _);
            throw new TimeoutException($"Request to {queueName} timed out after {timeoutMs}ms");
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}