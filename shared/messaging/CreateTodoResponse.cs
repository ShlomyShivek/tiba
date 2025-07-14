using Tiba.Shared.Model;

namespace Tiba.Shared.Messaging;

public class CreateTodoResponse : BaseResponse
{
    public Todo? CreatedTodo { get; set; }
}