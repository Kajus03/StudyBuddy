using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Services.UserSessionService;

public class UserSessionService : IUserSessionService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private UserId? _currentUserId;
    private string? _currentUsername;

    public UserSessionService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public UserId? GetCurrentUserId() => _currentUserId;
    public string? GetCurrentUsername() => _currentUsername;
    public void SetCurrentUsername(string username) => _currentUsername = username;

    public void SetCurrentUser(UserId userId) => _currentUserId = userId;
   public async Task<IUser?> GetUser(string username)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("StudyBuddy.API");
        var response = await httpClient.GetAsync($"api/v1/user/by-username/{username}");

        if (!response.IsSuccessStatusCode)
        {
            return null; 
        }

        IUser? user = await response.Content.ReadFromJsonAsync<User>();

        return user;
    }

    public async Task<bool> AuthenticateUser(string username, string password)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("StudyBuddy.API");
        var response = await httpClient.GetAsync($"api/v1/user/by-username/{username}");

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        IUser? user = await response.Content.ReadFromJsonAsync<User>();
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return false;
        }

        SetCurrentUsername(username);
        SetCurrentUser(user.Id);
        return true;
    }
}
