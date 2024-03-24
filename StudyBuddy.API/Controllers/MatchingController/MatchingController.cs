using Microsoft.AspNetCore.Mvc;
using StudyBuddy.API.Services.MatchingService;
using StudyBuddy.API.Services.UserService;
using StudyBuddy.Shared.DTOs;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.API.Controllers.MatchingController;

[ApiController]
[Route("api/v1/matching")]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;
    private readonly IUserService _userService;

    public MatchingController(IMatchingService matchingService, IUserService userService)
    {
        _matchingService = matchingService;
        _userService = userService;
    }
    [HttpPost("match-users")]
    public async Task<IActionResult> MatchUsers([FromBody] MatchDto matchDto)
    {
        await _matchingService.MatchUsersAsync(matchDto);
        return NoContent();
    }

    [HttpGet("is-matched/{currentUser:guid}/{otherUser:guid}")]
    public async Task<IActionResult> IsMatched(Guid currentUser, Guid otherUser)
    {
        bool isMatched = await _matchingService.IsMatchedAsync(UserId.From(currentUser), UserId.From(otherUser));
        return Ok(isMatched);
    }

    [HttpGet("match-history/{userId:guid}")]
    public async Task<IActionResult> GetMatchHistory(Guid userId)
    {
        UserId matchedUser = UserId.From(userId);
        IEnumerable<User> blocked = await _userService.GetBlockedUsers(matchedUser);

        List<UserId> blockedIds = blocked.Select(user => user.Id).ToList();

        IEnumerable<Match> matchHistory = await _matchingService.GetMatchHistoryAsync(UserId.From(userId));
        matchHistory = matchHistory.Where(m => (m.User1Id == matchedUser && !blockedIds.Contains(m.User2Id)) || (m.User2Id == matchedUser && !blockedIds.Contains(m.User1Id))).ToList();
        return Ok(matchHistory);
    }

    [HttpGet("is-requested-match/{currentUser:guid}/{otherUser:guid}")]
    public async Task<IActionResult> IsRequestedMatch(Guid currentUser, Guid otherUser)
    {
        bool isRequestedMatch =
            await _matchingService.IsRequestedMatchAsync(UserId.From(currentUser), UserId.From(otherUser));
        return Ok(isRequestedMatch);
    }
}
