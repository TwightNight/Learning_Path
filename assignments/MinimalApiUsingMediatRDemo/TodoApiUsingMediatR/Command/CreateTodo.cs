using MediatR;

namespace TodoApiUsingMediatR.Command;

public record CreateTodoCommand(string Name, bool IsComplete) : IRequest<Todo>;

public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, Todo>
{
    private readonly AppDbContext _dbContext;

    public CreateTodoCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Name = request.Name,
            IsComplete = request.IsComplete
        };

        _dbContext.Todos.Add(todo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return todo;
    }
}