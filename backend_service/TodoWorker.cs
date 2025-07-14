using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Tiba.Shared.Messaging;
using Tiba.BackendService.Handlers;
using Tiba.BackendService.Dal;


namespace Tiba.BackendService;

public class TodoWorker : BackgroundService
{
    private readonly ILogger<TodoWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public TodoWorker(ILogger<TodoWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare dead letter exchange. exchange is required for dead letter queues
            _channel.ExchangeDeclare(exchange: "todo.deadletter", type: ExchangeType.Direct, durable: true);

            // Declare dead letter queues
            _channel.QueueDeclare(
                queue: "todo.get_by_user.deadletter",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            _channel.QueueDeclare(
                queue: "todo.create.deadletter",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind dead letter queues to dead letter exchange
            _channel.QueueBind(queue: "todo.get_by_user.deadletter", exchange: "todo.deadletter", routingKey: "todo.get_by_user");
            _channel.QueueBind(queue: "todo.create.deadletter", exchange: "todo.deadletter", routingKey: "todo.create");

            // Declare main queues with dead letter configuration
            var getTodosQueueArgs = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "todo.deadletter"},
                {"x-dead-letter-routing-key", "todo.get_by_user"},
                {"x-max-retries", 3}
            };

            _channel.QueueDeclare(
                queue: "todo.get_by_user",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: getTodosQueueArgs);

            var createTodoQueueArgs = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "todo.deadletter"},
                {"x-dead-letter-routing-key", "todo.create"},
                {"x-max-retries", 3}
            };

            _channel.QueueDeclare(
                queue: "todo.create",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: createTodoQueueArgs);

            // Set up consumers
            SetupGetTodosConsumer();
            SetupCreateTodoConsumer();

            _logger.LogInformation("RabbitMQ initialized and consumers started");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ");
            throw;
        }
    }

    private void SetupGetTodosConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var todoRepository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
            var getTodosHandler = new GetTodosHandler(_logger, todoRepository);
            
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<GetTodosByUserIdRequest>(messageJson);
                if (request == null || request.UserId <= 0)
                {
                    _logger.LogWarning("Invalid GetTodosByUserIdRequest received: {Message}. Rejecting to dead letter queue.", messageJson);
                    _channel?.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                var response = await getTodosHandler.HandleAsync(request);

                await SendReply(ea.BasicProperties, JsonSerializer.Serialize(response));
                _channel?.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                var retryCount = GetRetryCount(ea.BasicProperties);
                _logger.LogError(ex, "Error processing get todos request. Retry count: {RetryCount}", retryCount);
                
                if (retryCount >= 3)
                {
                    _logger.LogError("Max retries exceeded for get todos request. Sending to dead letter queue.");
                    _channel?.BasicReject(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogInformation("Rejecting message for retry. Retry count: {RetryCount}", retryCount + 1);
                    _channel?.BasicReject(ea.DeliveryTag, true);
                }
            }
        };

        _channel?.BasicConsume(queue: "todo.get_by_user", autoAck: false, consumer: consumer);
    }

    private void SetupCreateTodoConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var todoRepository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
            var createTodoHandler = new CreateTodoHandler(_logger, todoRepository);
            
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                var request = JsonSerializer.Deserialize<CreateTodoRequest>(messageJson);
                if (request == null || request.Todo.UserId <= 0)
                {
                    _logger.LogWarning("Invalid CreateTodoRequest received: {Message}. Rejecting to dead letter queue.", messageJson);
                    _channel?.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                var response = await createTodoHandler.HandleAsync(request);

                await SendReply(ea.BasicProperties, JsonSerializer.Serialize(response));
                _channel?.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                var retryCount = GetRetryCount(ea.BasicProperties);
                _logger.LogError(ex, "Error processing create todo request. Retry count: {RetryCount}", retryCount);
                
                if (retryCount >= 3)
                {
                    _logger.LogError("Max retries exceeded for create todo request. Sending to dead letter queue.");
                    _channel?.BasicReject(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogInformation("Rejecting message for retry. Retry count: {RetryCount}", retryCount + 1);
                    _channel?.BasicReject(ea.DeliveryTag, true);
                }
            }
        };

        _channel?.BasicConsume(queue: "todo.create", autoAck: false, consumer: consumer);
    }

    private async Task SendReply(IBasicProperties requestProperties, string responseMessage)
    {
        if (requestProperties.ReplyTo != null && requestProperties.CorrelationId != null)
        {
            var replyProperties = _channel?.CreateBasicProperties();
            if (replyProperties != null)
            {
                replyProperties.CorrelationId = requestProperties.CorrelationId;

                var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                _channel?.BasicPublish(exchange: "", routingKey: requestProperties.ReplyTo, basicProperties: replyProperties, body: responseBytes);

                _logger.LogInformation("Sent reply to {ReplyTo} with correlation {CorrelationId}", requestProperties.ReplyTo, requestProperties.CorrelationId);
            }
        }
        await Task.CompletedTask;
    }

    private static int GetRetryCount(IBasicProperties properties)
    {
        if (properties.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var retryCountObj))
        {
            if (retryCountObj is byte[] retryCountBytes)
            {
                var retryCountStr = Encoding.UTF8.GetString(retryCountBytes);
                if (int.TryParse(retryCountStr, out var retryCount))
                {
                    return retryCount;
                }
            }
        }
        return 0;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}