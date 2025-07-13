using Microsoft.Extensions.Logging;
using tiba.rest.messaging;
using tiba.rest.model;

namespace backend_service;

public class CreateTodoHandler
{
    private readonly ILogger<TodoWorker> _logger;
    private readonly ITodoRepository _todoRepository;

    public CreateTodoHandler(ILogger<TodoWorker> logger, ITodoRepository todoRepository)
    {
        _logger = logger;
        _todoRepository = todoRepository;
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
