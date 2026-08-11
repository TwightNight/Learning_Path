using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.CreatePost;

public sealed class CreatePostMapper: Mapper<CreatePostRequest, CreatePostResponse, Post>
{

    public override CreatePostResponse FromEntity(Post entity)
    {
        return new CreatePostResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content
        };
    }
}