using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.UpdatePost;

public sealed class UpdatePostMapper : Mapper<UpdatePostRequest, UpdatePostResponse, Post>
{
    public override UpdatePostResponse FromEntity(Post entity)
    {
        return new UpdatePostResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            LastModified = entity.LastModified
        };
    }
}