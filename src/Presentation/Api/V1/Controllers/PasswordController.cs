using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class PasswordController : ControllerBase

  {
    private readonly IPasswordService _passwordService;
    private readonly IUserService _userService;

    public PasswordController(IPasswordService passwordService)
    {
      _passwordService = passwordService;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
      var result = await _passwordService.RequestPasswordResetAsync(request.Email);
      return Ok(new { success = result });
    }

    [HttpPost("reset-password")]
    [Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
      var result = await _passwordService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
      if (!result)
      {
        return BadRequest("Invalid or expired reset token");
      }
      return Ok(new { success = true });
    }
  }
}