using Tiba.Shared.Model;

namespace Tiba.Shared.Messaging;

public class GetTodosResponse : BaseResponse
{
    public List<Todo> Todos { get; set; } = [];
}