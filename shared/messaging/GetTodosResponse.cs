using tiba.rest.model;

namespace tiba.rest.messaging;

public class GetTodosResponse : BaseResponse
{
    public List<Todo> Todos { get; set; } = [];
}