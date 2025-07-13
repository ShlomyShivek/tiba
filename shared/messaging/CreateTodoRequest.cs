using tiba.rest.model;

namespace tiba.rest.messaging;

public class CreateTodoRequest
{
    public Todo Todo { get; set; } = new();
}