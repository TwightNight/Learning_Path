using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TodoApiUsingMediatR.Query;

public record GetToDosQuery : IRequest<List<Todo>>;

public class GetToDosQueryHandler : IRequestHandler<GetToDosQuery, List<Todo>>
{
    private readonly AppDbContext _dbContext;

    public GetToDosQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Todo>> Handle(GetToDosQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Todos.ToListAsync(cancellationToken);
    }
}