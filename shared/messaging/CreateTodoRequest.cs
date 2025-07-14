using Tiba.Shared.Model;

namespace Tiba.Shared.Messaging;

public class CreateTodoRequest
{
    public Todo Todo { get; set; } = new();
}