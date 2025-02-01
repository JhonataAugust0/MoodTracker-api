

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Presentation.Controllers
{
  [ApiController]
  [Route("api/notification")]
  [Authorize]
  public class NotificationsController : ControllerBase
  {
    private readonly IRedisService _redisService;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(
      IRedisService redisService,
      IUserService userService,
      ICurrentUserService currentUserService)
    {
      _redisService = redisService;
      _userService = userService;
      _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([Required] int UserId)
    {
      var notifications = await _redisService.GetNotifications(UserId);
      return Ok(notifications);
    }
    
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkNotificationsAsRead()
    {
      await _redisService.RemoveNotifications(_currentUserService.UserId);
      return NoContent();
    }
    
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification([Required] string notificationId)
    {
      var removed = await _redisService.RemoveNotification(_currentUserService.UserId, notificationId);
      return removed ? NoContent() : NotFound();
    }
  }
}