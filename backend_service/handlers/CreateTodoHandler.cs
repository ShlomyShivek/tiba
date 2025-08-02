using Microsoft.Extensions.Logging;
using Tiba.Shared.Messaging;
using Tiba.BackendService.Dal;

namespace Tiba.BackendService.Handlers;

public class CreateTodoHandler : BaseTodoHandler
{
    public CreateTodoHandler(ILogger<TodoWorker> logger, ITodoRepository todoRepository) : base(logger, todoRepository)
    {
    }

    public async Task<CreateTodoResponse> HandleAsync(CreateTodoRequest message)
    {
        _logger.LogInformation("Received create todo request for user: {UserId}", message.Todo.UserId);

        // var maxRetries = 5;
        // var retryDelay = TimeSpan.FromSeconds(0.5);

        // var dbCreated = 0;
        // while (dbCreated < maxRetries)
        // {
        try
        {
            var createdTodo = await _todoRepository.CreateTodoAsync(message.Todo);
            return new CreateTodoResponse
            {
                CreatedTodo = createdTodo,
                Success = true
            };

        }
        catch (Exception ex)
        {
            // dbCreated++;
            // _logger.LogWarning("Error creating todo for user {UserId}. Retry attempts: {Attempt} of {MaxRetries}", message.Todo.UserId, dbCreated, maxRetries);
            // if (dbCreated >= maxRetries)
            // {
            _logger.LogError(ex, "Failed to create todo for user {UserId} after multiple attempts", message.Todo.UserId);
            return new CreateTodoResponse
            {
                Success = false,
                ErrorMessage = "Failed to create todo after multiple attempts"
            };
            // }
            // Task.Delay(retryDelay).Wait();
        }
        // dbCreated++;
    }

    // return new CreateTodoResponse
    // {
    //     Success = false,
    //     ErrorMessage = "Failed to create todo"
    // };
}
    

