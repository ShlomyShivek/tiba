using Microsoft.Extensions.Logging;
using Tiba.Shared.Messaging;
using Tiba.BackendService.Dal;

namespace Tiba.BackendService.Handlers;

public class GetTodosHandler : BaseTodoHandler
{
    public GetTodosHandler(ILogger<TodoWorker> logger, ITodoRepository todoRepository) : base(logger, todoRepository)
    {
    }

    public async Task<GetTodosResponse> HandleAsync(GetTodosByUserIdRequest message)
    {
        _logger.LogInformation("Received get todos request for user: {UserId}", message.UserId);

        try
        {
            var todos = await _todoRepository.GetTodosByUserIdAsync(message.UserId);

            return new GetTodosResponse
            {
                Success = true,
                Todos = todos.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get todos for user {UserId}", message.UserId);
            return new GetTodosResponse
            {
                Success = false,
                ErrorMessage = "Failed to retrieve todos"
            };
        }
    }
}
