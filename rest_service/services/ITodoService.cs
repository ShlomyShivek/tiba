using Tiba.Shared.Model;

namespace Tiba.Rest.Services;

public interface ITodoService
{
    Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId);
    Task<Todo> CreateTodoAsync(Todo todo);
}