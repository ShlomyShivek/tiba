using Tiba.Shared.Model;
using Tiba.Shared.Messaging;

namespace Tiba.Rest.Services;

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
            request, "todo.get_by_user");
        
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
            request, "todo.create");
        
        if (!response.Success || response.CreatedTodo == null)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Failed to create todo");
        }
        
        return response.CreatedTodo;
    }
}
