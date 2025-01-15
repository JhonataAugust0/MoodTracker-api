using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UserController : ControllerBase

  {
    private readonly IUserService _userService;

    public UserController( IUserService userService)
    {
      _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
      var result = await _userService.RegisterUserAsync(request.Email, request.Password, request.Name);
      // if (result.Email != Nullable && result.Name)
      // {
      //   return BadRequest(result.Error);
      // }
      return Ok(result);
    }

  //   [HttpDelete("delete")]
  //   [Authorize]
  //   public async Task<IActionResult> DeleteUser([FromBody] LogoutRequestDTO request)
  //   {
  //     var result = await _authService.RevokeTokenAsync(request.RefreshToken);
  //     return Ok(new { success = result });
  //   }
  }
}