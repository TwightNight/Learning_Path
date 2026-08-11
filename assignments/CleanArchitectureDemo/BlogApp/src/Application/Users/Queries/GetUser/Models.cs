namespace BlogApp.Application.Users.GetUser;

public class GetUserRequest
{
    public int Id {get; set;}
}
public class GetUserResponse
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string FullName {get; set;} = default!;
    public string Email { get; set; } = default!;
}