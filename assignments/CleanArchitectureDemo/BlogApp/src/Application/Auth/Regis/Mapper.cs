using FastEndpoints;

namespace BlogApp.Application.Auth.Regis;

public sealed class RegisMapper : Mapper<RegisRequest, RegisResponse, Domain.Entities.User>
{
    // Dùng khi bạn muốn map trực tiếp Request -> Entity trong Endpoint
    // (không hash password ở đây vì Mapper không có DI service)
    public override Domain.Entities.User ToEntity(RegisRequest req) => new()
    {
        FirstName = req.FirstName,
        LastName = req.LastName,
        UserName = req.UserName,
        Email = req.Email,
        PasswordHash = req.Password
    };

    public override RegisResponse FromEntity(Domain.Entities.User entity) => new()
    {
        Email = entity.Email,
        // UserName = entity.UserName
        // Token được gán sau ở Endpoint, vì cần service JWT
    };
}