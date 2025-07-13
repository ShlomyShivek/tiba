using Microsoft.Extensions.Logging;
using backend_service.dal;

namespace backend_service.handlers;

public abstract class BaseTodoHandler
{
    public BaseTodoHandler(ILogger<TodoWorker> logger, ITodoRepository todoRepository)
    {
        _logger = logger;
        _todoRepository = todoRepository;
    }

    protected readonly ILogger<TodoWorker> _logger;
    protected readonly ITodoRepository _todoRepository;

}
