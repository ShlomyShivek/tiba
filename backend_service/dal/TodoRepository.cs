using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiba.rest.model;

namespace backend_service.dal;

public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;
    private readonly ILogger<TodoRepository> _logger;

    public TodoRepository(TodoDbContext context, ILogger<TodoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Todo>> GetTodosByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting todos for user {UserId}", userId);
            
            return await _context.Todos
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todos for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Todo?> GetTodoByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting todo by id {TodoId}", id);
            
            return await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todo by id {TodoId}", id);
            throw;
        }
    }

    public async Task<Todo> CreateTodoAsync(Todo todo)
    {
        try
        {
            _logger.LogInformation("Creating todo for user {UserId}", todo.UserId);
            
            // Ensure CreatedAt is set
            if (todo.CreatedAt == default)
            {
                todo.CreatedAt = DateTime.UtcNow;
            }

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created todo with id {TodoId} for user {UserId}", todo.Id, todo.UserId);
            return todo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo for user {UserId}", todo.UserId);
            throw;
        }
    }

    public async Task<Todo?> UpdateTodoAsync(Todo todo)
    {
        try
        {
            _logger.LogInformation("Updating todo {TodoId} for user {UserId}", todo.Id, todo.UserId);
            
            var existingTodo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todo.Id && t.UserId == todo.UserId);
            
            if (existingTodo == null)
            {
                _logger.LogWarning("Todo {TodoId} not found for user {UserId}", todo.Id, todo.UserId);
                return null;
            }

            // Update properties
            existingTodo.Title = todo.Title;
            existingTodo.IsCompleted = todo.IsCompleted;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated todo {TodoId} for user {UserId}", todo.Id, todo.UserId);
            return existingTodo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating todo {TodoId} for user {UserId}", todo.Id, todo.UserId);
            throw;
        }
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting todo {TodoId}", id);
            
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                _logger.LogWarning("Todo {TodoId} not found for deletion", id);
                return false;
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted todo {TodoId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo {TodoId}", id);
            throw;
        }
    }
}
