using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TodoApiUsingMediatR.Query;

public record GetTodoByIdQuery(int Id) : IRequest<Todo?>;

public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, Todo?>
{
    private readonly AppDbContext _dbContext;

    public GetTodoByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Todo?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Todos.FindAsync(request.Id, cancellationToken);
    }
}