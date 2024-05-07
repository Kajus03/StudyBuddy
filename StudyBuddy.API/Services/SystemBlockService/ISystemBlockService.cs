using StudyBuddy.Shared.DTOs.systemBlockDtos;
using StudyBuddy.Shared.Models;

namespace StudyBuddy.API.Services.SystemBlockService
{
    public interface ISystemBlockService
    {
        public Task<SystemBlockResponse?> GetSystemBlockAsync(Guid id);
        public Task<IEnumerable<SystemBlockResponse>> GetSystemBlocksAsync();
        public Task<bool> RemoveSystemBlockAsync(Guid id);
        public Task<SystemBlockResponse?> CreateSystemBlockAsync(SystemBlockRequest block);
    }
}
