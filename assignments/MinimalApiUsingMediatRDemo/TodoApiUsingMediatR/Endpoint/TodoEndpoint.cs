using MediatR;
using TodoApiUsingMediatR.Command;
using TodoApiUsingMediatR.Model.DTOs;
using TodoApiUsingMediatR.Query;

namespace TodoApiUsingMediatR.Endpoint;

public static class TodoEndpoint
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var todos = app.MapGroup("/todos");

        todos.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetToDosQuery();
            var todos = await mediator.Send(query);
            return Results.Ok(todos);
        });

        todos.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var query = new GetTodoByIdQuery(id);
            var todo = await mediator.Send(query);
            return todo is not null ? Results.Ok(todo) : Results.NotFound();
        });

        todos.MapPost("/", async (TodoDto todo, IMediator mediator) =>
        {
            var command = new CreateTodoCommand(todo.Name, todo.IsComplete);
            var createdTodo = await mediator.Send(command);
            return Results.Created($"/todos/{createdTodo.Id}", createdTodo);
        });

        todos.MapPut("/{id:int}", async (int id, TodoDto todo, IMediator mediator) =>
        {
            var command = new UpdateTodoCommand(id, todo.Name, todo.IsComplete);
            var updated = await mediator.Send(command);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        todos.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            var command = new DeleteTodoCommand(id);
            var deleted = await mediator.Send(command);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}