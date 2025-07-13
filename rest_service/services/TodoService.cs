using tiba.rest.model;
using tiba.rest.messaging;

namespace tiba.rest.services;

public class TodoService : ITodoService
{
    private readonly IRabbitMqClient _rabbitMqClient;

    public TodoService(IRabbitMqClient rabbitMqClient)
    {
        _rabbitMqClient = rabbitMqClient;
    }

    public async Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId)
    {
        var request = new GetTodosByUserIdRequest { UserId = userId };
        var response = await _rabbitMqClient.SendRequestAsync<GetTodosByUserIdRequest, GetTodosResponse>(
            request, "todo.get_by_user", 30000);
        
        if (!response.Success)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Failed to get todos");
        }
        
        return response.Todos;
    }

    public async Task<Todo> CreateTodoAsync(Todo todo)
    {
        var request = new CreateTodoRequest { Todo = todo };
        var response = await _rabbitMqClient.SendRequestAsync<CreateTodoRequest, CreateTodoResponse>(
            request, "todo.create", 30000);
        
        if (!response.Success || response.CreatedTodo == null)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Failed to create todo");
        }
        
        return response.CreatedTodo;
    }
}
