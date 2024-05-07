using System.Linq.Expressions;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using StudyBuddy.API.Data.Repositories.SystemBlockRepository;
using StudyBuddy.Shared.DTOs.systemBlockDtos;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Services.SystemBlockService
{
    public class SystemBlockService : ISystemBlockService
    {
        private readonly ISystemBlockRepository _systemBlockRepository;
        public SystemBlockService(ISystemBlockRepository systemBlockRepository)
        {
            _systemBlockRepository = systemBlockRepository;
        }
        public async Task<SystemBlockResponse?> CreateSystemBlockAsync(SystemBlockRequest blockRequest)
        {
            SystemBlock? systemBlock = await _systemBlockRepository.GetSystemBlockByUserId(UserId.From(blockRequest.BlockedUserId));

            if (systemBlock != null)
            {
                throw new Exception("User already blocked");
            }

            SystemBlock newBlock = SystemBlock.fromDto(blockRequest);
            SystemBlock? added = await _systemBlockRepository.AddSystemBlock(newBlock);

            if (added == null)
            {
                throw new Exception("Failed to block");
            }
            return SystemBlock.toDto(newBlock);
        }

        public async Task<SystemBlockResponse?> GetSystemBlockAsync(Guid id)
        {
            SystemBlock? block = await _systemBlockRepository.GetSystemBlockById(id);
            return block != null
                ? SystemBlock.toDto(block)
                : throw new Exception("Failed to find block");
        }

        public async Task<IEnumerable<SystemBlockResponse>> GetSystemBlocksAsync()
        {
            IEnumerable<SystemBlock> blocks = await _systemBlockRepository.GetSystemBlocks();

            return blocks.Select((block) => SystemBlock.toDto(block));
        }

        public async Task<bool> RemoveSystemBlockAsync(Guid id)
        {
            return await _systemBlockRepository.DeleteSystemBlock(id);
        }
    }
}
