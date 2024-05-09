using Microsoft.CodeAnalysis;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Data.Repositories.SystemBlockRepository
{
    public interface ISystemBlockRepository
    {
        Task<IEnumerable<SystemBlock>> GetSystemBlocks();
        Task<SystemBlock?> GetSystemBlockById(Guid id);
        Task<SystemBlock?> GetSystemBlockByUserId(UserId id);
        Task<SystemBlock?> AddSystemBlock(SystemBlock systemBlock);
        Task<bool> DeleteSystemBlock(Guid id);
    }
}
