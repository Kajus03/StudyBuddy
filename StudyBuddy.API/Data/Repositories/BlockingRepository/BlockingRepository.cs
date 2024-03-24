using Microsoft.EntityFrameworkCore;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Data.Repositories.BlockingRepository
{
    public class BlockingRepository : IBlockingRepository
    {
        private readonly StudyBuddyDbContext _context;

        public BlockingRepository(StudyBuddyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BlockUser(UserId blockingUserId, UserId UserToBeBlockedId)
        {
            Block block = new()
            {
                SenderId = blockingUserId,
                ReceiverId = UserToBeBlockedId
            };

            await _context.Blockings.AddAsync(block);
            _context.SaveChanges();
            return true;
        }

            public async Task<IEnumerable<User>> getBlockedUsers(UserId id)
            {
            List<UserId> usersIds = await _context.Blockings
                .Where(b => b.SenderId == id)
                .Select(b => b.ReceiverId)
                .ToListAsync();

            IEnumerable<User> users = _context.Users
                .Where(user => usersIds.Contains(user.Id))
                .ToList();

            return users;
            return new List<User>();
            }

        public async Task<bool> UnblockUser(UserId blockingUserId, UserId UserToBeBlockedId)
        {
            Block block = await _context.Blockings.FirstAsync(b => b.SenderId == blockingUserId && b.ReceiverId == UserToBeBlockedId);

            _context.Blockings.Remove(block);
            _context.SaveChanges();
            return true;
        }
    }
}
