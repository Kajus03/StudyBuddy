using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Data.Repositories.BlockingRepository
{
    public interface IBlockingRepository
    {
        Task<bool> BlockUser(UserId blockingUserId, UserId UserToBeBlockedId);
        Task<bool> UnblockUser(UserId blockingUserId, UserId UserToBeBlockedId);
        Task<IEnumerable<User>> getBlockedUsers(UserId id);

    }
}
