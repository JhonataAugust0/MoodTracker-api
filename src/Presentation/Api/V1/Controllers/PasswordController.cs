using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
{
  [ApiController]
  [Route("api/password")]
  public class PasswordController : ControllerBase
  {
    private readonly IPasswordService _passwordService;

    public PasswordController(IPasswordService passwordService)
    {
      _passwordService = passwordService;
    }

    [HttpPost("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
      var result = await _passwordService.RequestPasswordResetAsync(request.Email);
      return Ok(new { success = result });
    }

    [HttpPost("change")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
    {
      var result = await _passwordService.ChangePasswordAsync(request.Email, request.Password, request.NewPassword);
      if (!result)
      {
        return Unauthorized("Invalid current password");
      }
      return Ok(new { success = true });
    }
  }
}