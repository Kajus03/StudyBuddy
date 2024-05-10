using StudyBuddy.Shared.ValueObjects;
using StudyBuddy.Shared.Abstractions;

namespace StudyBuddy.Services.UserSessionService;

public interface IUserSessionService
{
    UserId? GetCurrentUserId();
    void SetCurrentUser(UserId userId);
    Task<bool> AuthenticateUser(string username, string password);
    Task<IUser?> GetUser(string username);
}
