using tiba.rest.model;

namespace backend_service;

public interface ITodoRepository
{
    Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId);
    Task<Todo?> GetTodoByIdAsync(int id);
    Task<Todo> CreateTodoAsync(Todo todo);
    Task<Todo?> UpdateTodoAsync(Todo todo);
    Task<bool> DeleteTodoAsync(int id);
}