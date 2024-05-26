using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StudyBuddy.API.Controllers.AdminController;
using StudyBuddy.API.Data;
using StudyBuddy.API.Data.Repositories.BlockingRepository;
using StudyBuddy.API.Data.Repositories.SystemBlockRepository;
using StudyBuddy.API.Data.Repositories.UserRepository;
using StudyBuddy.API.Services.SystemBlockService;
using StudyBuddy.API.Services.UserService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.DTOs.systemBlockDtos;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddyTests.Controllers
{
    public class AdminControllerITests {
        private readonly StudyBuddyDbContext _dbContext;
        private readonly AdminController _adminController;
        private readonly ILogger<UserService> _logger;
        private readonly ISystemBlockService _systemBlockService;
        private readonly IUserService _userService;
        private readonly ISystemBlockRepository _systemBlockRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBlockingRepository _blockingRepository;

        public AdminControllerITests()
        {
            DbContextOptions<StudyBuddyDbContext> options = new DbContextOptionsBuilder<StudyBuddyDbContext>()
                .UseInMemoryDatabase("AdminControllerTests")
                .Options;

            _logger = Substitute.For<ILogger<UserService>>();
            _dbContext = new StudyBuddyDbContext(options);
            _systemBlockRepository = new SystemBlockRepository(_dbContext);
            _systemBlockService = new SystemBlockService(_systemBlockRepository);
            _userRepository = new UserRepository(_dbContext);
            _blockingRepository = new BlockingRepository(_dbContext);

            _userService = new UserService(_userRepository, _blockingRepository, _logger);
            _adminController = new AdminController(_systemBlockService, _userService);
        }

        
        [Fact]
        public async Task GetSystemBlock_ReturnsOkResult_WithSystemBlock()
        {
            UserId userId = UserId.From(Guid.Parse("00000000-0000-0000-0000-111111111111"));
            Guid blockReasonId = Guid.Parse("00000000-0000-0000-0000-333333333333");
            Guid blockId = Guid.Parse("00000000-0000-0000-0000-222222222222");
            UserFlags userFlags = UserFlags.Admin;
            UserTraits userTraits = new();
            User user = new(userId, "User", "secret", userFlags, userTraits, new List<string>());
            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            
            SystemBlockReason reason = new()
            {
                Id = blockReasonId, 
                Reason = "You've been a bad boy ;)"
            };

            SystemBlock systemBlock = new()
            {
                Id = blockId,
                BlockedUntil = DateTime.Today,
                BlockedUserId = userId,
                SystemBlockReasonId = blockReasonId
            };

            await _dbContext.AddAsync(reason);
            await _dbContext.AddAsync(systemBlock);
            await _dbContext.SaveChangesAsync();

            var result = await _adminController.GetSystemBlock(blockId) as OkObjectResult;
            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetSystemBlocks_ReturnsOkResult_WithMultipleSystemBlocks()
        {
            // Arrange
            UserId userId1 = UserId.From(Guid.Parse("00000000-0000-0000-0000-777777777777"));
            UserId userId2 = UserId.From(Guid.Parse("00000000-0000-0000-0000-888888888888"));
            Guid blockReasonId1 = Guid.Parse("00000000-0000-0000-0000-999999999999");
            Guid blockReasonId2 = Guid.Parse("00000000-0000-0000-0000-444444444444");
            Guid blockId1 = Guid.Parse("00000000-0000-0000-0000-555555555555");
            Guid blockId2 = Guid.Parse("00000000-0000-0000-0000-666666666666");

            User user1 = new(userId1, "User1", "secret1", UserFlags.Admin, new UserTraits(), new List<string>());
            User user2 = new(userId2, "User2", "secret2", UserFlags.Admin, new UserTraits(), new List<string>());

            await _dbContext.AddAsync(user1);
            await _dbContext.AddAsync(user2);
            await _dbContext.SaveChangesAsync();

            SystemBlockReason reason1 = new() { Id = blockReasonId1, Reason = "Reason 1" };
            SystemBlockReason reason2 = new() { Id = blockReasonId2, Reason = "Reason 2" };

            SystemBlock systemBlock1 = new() { Id = blockId1, BlockedUntil = DateTime.Today, BlockedUserId = userId1, SystemBlockReasonId = blockReasonId1 };
            SystemBlock systemBlock2 = new() { Id = blockId2, BlockedUntil = DateTime.Today.AddDays(1), BlockedUserId = userId2, SystemBlockReasonId = blockReasonId2 };

            await _dbContext.AddAsync(reason1);
            await _dbContext.AddAsync(reason2);
            await _dbContext.AddAsync(systemBlock1);
            await _dbContext.AddAsync(systemBlock2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _adminController.GetSystemBlocks() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var collection = result.Value as IEnumerable<SystemBlockResponse>;

            Assert.NotNull(collection);
            Assert.NotEmpty(collection);
        }


        [Fact]
        public async Task CreateSystemBlock_ReturnsOkResult_WithCreatedSystemBlock()
        {
            // Arrange
            UserId userId = UserId.From(Guid.NewGuid());
            User user = new(userId, "User", "secret", UserFlags.Admin, new UserTraits(), new List<string>());
            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            SystemBlockReasonRequest reason = new() { Reason = "Reason" };

            SystemBlockRequest request = new()
            {
                BlockedUntil = DateTime.Today.AddDays(1),
                BlockedUserId = userId.Value,
                SystemBlockReason = reason 
            };

            // Act
            var result = await _adminController.CreateSystemBlock(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var createdBlock = result.Value as SystemBlockResponse;
            Assert.NotNull(createdBlock);
            Assert.Equal(userId, createdBlock.BlockedUserId);
        }

        [Fact]
        public async Task RemoveSystemBlock_ReturnsOkResult_WhenBlockExists()
        {
            // Arrange
            UserId userId = UserId.From(Guid.NewGuid());
            User user = new(userId, "User", "secret", UserFlags.Admin, new UserTraits(), new List<string>());
            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            Guid blockReasonId = Guid.NewGuid();
            SystemBlockReason reason = new() { Id = blockReasonId, Reason = "Reason" };
            await _dbContext.AddAsync(reason);
            await _dbContext.SaveChangesAsync();

            Guid blockId = Guid.NewGuid();
            SystemBlock systemBlock = new()
            {
                Id = blockId,
                BlockedUntil = DateTime.Today.AddDays(1),
                BlockedUserId = userId,
                SystemBlockReasonId = blockReasonId
            };
            await _dbContext.AddAsync(systemBlock);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _adminController.RemoveSystemBlock(blockId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            UserId userId = UserId.From(Guid.NewGuid());
            User user = new(userId, "User", "secret", UserFlags.Admin, new UserTraits(), new List<string>());
            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _adminController.DeleteUser(userId.Value) as OkResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            // Verify the user has been removed from the database
            var userInDb = await _dbContext.Users.FindAsync(userId);
            Assert.Null(userInDb);
        }
    }
}
