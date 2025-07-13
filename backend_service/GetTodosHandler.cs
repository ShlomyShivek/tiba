using Microsoft.Extensions.Logging;
using tiba.rest.messaging;
using tiba.rest.model;

namespace backend_service;

public class GetTodosHandler
{
    private readonly ILogger<TodoWorker> _logger;
    private readonly ITodoRepository _todoRepository;

    public GetTodosHandler(ILogger<TodoWorker> logger, ITodoRepository todoRepository)
    {
        _logger = logger;
        _todoRepository = todoRepository;
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
