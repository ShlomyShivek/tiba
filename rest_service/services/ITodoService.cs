using Tiba.Shared.Model;

namespace Tiba.Rest.Services;

/// <summary>
/// Service interface for managing Todo operations.
/// Provides methods for retrieving and creating todos with async support.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Retrieves all todos for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose todos to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of todos for the specified user.</returns>
    Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId);

    /// <summary>
    /// Creates a new todo item.
    /// </summary>
    /// <param name="todo">The todo item to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created todo item with any assigned identifiers.</returns>
    Task<Todo> CreateTodoAsync(Todo todo);
}