namespace BlogApp.Application.User.GetUsers;


public class GetUsersRequest {}
public class GetUsersResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }

}