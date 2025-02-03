using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
    [ApiController]
    [Route("api/notifications")]
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
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                int userId = _currentUserService.UserId;
                var notifications = await _redisService.GetNotifications(userId);

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar notificações", error = ex.Message });
            }
        }

        [HttpPost("mark-read"), HttpOptions("mark-read")]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            try
            {
                int userId = _currentUserService.UserId;
                await _redisService.RemoveNotifications(userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao marcar notificações como lidas", error = ex.Message });
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                int userId = _currentUserService.UserId;
                bool removed = await _redisService.RemoveNotification(userId, notificationId);

                if (!removed)
                    return NotFound(new { message = "Notificação não encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao deletar notificação", error = ex.Message });
            }
        }
    }
}
