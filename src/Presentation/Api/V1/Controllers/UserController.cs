using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
  [ApiController]
  [Route("api/users")]
  public class UserController : ControllerBase
  {
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserController(IUserService userService, ICurrentUserService currentUserService)
    {
      _userService = userService;
      _currentUserService = currentUserService;
    }

    [HttpPost,HttpOptions]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
      try
      {
        var result = await _userService.RegisterUserAsync(request.Email, request.Password, request.Name);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
      try
      {
        var userId = _currentUserService.UserId;
        if (userId == null)
          return Unauthorized(new { error = "User ID not found in token." });

        await _userService.DeleteUserAsync(userId);
        return Ok(new { message = "User deleted successfully" });
      }
      catch (NotFoundException)
      {
        return NotFound(new { error = "User not found" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = "An error occurred while processing your request.", details = ex.Message });
      }
    }
  }
}