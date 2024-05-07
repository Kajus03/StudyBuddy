using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Data.Repositories.SystemBlockRepository
{
    public class SystemBlockRepository : ISystemBlockRepository
    {
        private readonly StudyBuddyDbContext _context;

        public SystemBlockRepository(StudyBuddyDbContext context)
        {
            _context = context;
        }
        public async Task<SystemBlock?> AddSystemBlock(SystemBlock systemBlock)
        {
            try
            {
                await _context.AddAsync(systemBlock);
                await _context.SaveChangesAsync();
                return systemBlock;
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> DeleteSystemBlock(Guid id)
        {
            SystemBlock? block = await _context.SystemBlocks.FirstOrDefaultAsync(block => block.Id == id);
            if (block == null)
            {
                return false;
            }
            try
            {
                _context.SystemBlocks.Remove(block);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<SystemBlock?> GetSystemBlockById(Guid id)
        {
            return await _context.SystemBlocks.Include(block => block.SystemBlockReason).FirstOrDefaultAsync(block => block.Id == id);
        }
        public async Task<SystemBlock?> GetSystemBlockByUserId(UserId id)
        {
            return await _context.SystemBlocks.FirstOrDefaultAsync(block => block.BlockedUserId == id);
        }
        public async Task<IEnumerable<SystemBlock>> GetSystemBlocks()
        {
            return await _context.SystemBlocks.Include(block => block.SystemBlockReason).ToListAsync();
        }
    }
}
