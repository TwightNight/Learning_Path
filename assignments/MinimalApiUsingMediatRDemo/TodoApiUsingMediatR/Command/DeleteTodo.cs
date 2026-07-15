using MediatR;

namespace TodoApiUsingMediatR.Command;

public record DeleteTodoCommand(int Id) : IRequest<bool>;

public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public DeleteTodoCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _dbContext.Todos.FindAsync(request.Id, cancellationToken);
        if (todo == null)
        {
            return false;
        }

        _dbContext.Todos.Remove(todo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}