using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
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

    [HttpPost()]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
      var result = await _userService.RegisterUserAsync(request.Email, request.Password, request.Name);
      return Ok(result);
    }

    [HttpDelete()]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
      var user = _currentUserService.UserId;
      if (user == null)
      {
        return Unauthorized("User ID not found in token.");
      }

      await _userService.DeleteUserAsync(user);
      return Ok("User deleted successfully");

    }
  }
}