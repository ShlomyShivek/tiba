using Microsoft.Extensions.Logging;
using BackendService.Dal;

namespace BackendService.Handlers;

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
