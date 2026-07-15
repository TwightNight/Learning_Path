using MediatR;

namespace TodoApiUsingMediatR.Command;

public record UpdateTodoCommand(int Id, string? Name, bool? IsComplete) : IRequest<bool>;

public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public UpdateTodoCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _dbContext.Todos.FindAsync(request.Id, cancellationToken);
        if (todo == null)
        {
            return false;
        }

        if (request.Name != null)
        {
            todo.Name = request.Name;
        }

        if (request.IsComplete.HasValue)
        {
            todo.IsComplete = request.IsComplete.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}