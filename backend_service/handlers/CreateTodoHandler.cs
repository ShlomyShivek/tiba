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
            _logger.LogError(ex, "Failed to create todo for user {UserId}", message.Todo.UserId);
            return new CreateTodoResponse
            {
                Success = false,
                ErrorMessage = "Failed to create todo"
            };
        }
    }
}
