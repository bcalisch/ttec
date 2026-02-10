namespace Backend.Api.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string DisplayName { get; }
}

public class DevCurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string Email => "dev@geoops.local";
    public string DisplayName => "Dev User";
}
