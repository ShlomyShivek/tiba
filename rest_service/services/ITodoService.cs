using tiba.rest.model;

namespace tiba.rest.services;

public interface ITodoService
{
    Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId);
    Task<Todo> CreateTodoAsync(Todo todo);
}