using tiba.rest.model;

namespace tiba.rest.messaging;

public class CreateTodoResponse : BaseResponse
{
    public Todo? CreatedTodo { get; set; }
}