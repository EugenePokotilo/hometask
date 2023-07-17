namespace GameServer;

public class CurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserIfo GetUserInfo() => new UserIfo()
    {
        UserId = long.Parse(_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserId").Value),
        Udid = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "Udid").Value
    };
}

public class UserIfo
{
    public long UserId { get; set; }
    public string Udid { get; set; }
}