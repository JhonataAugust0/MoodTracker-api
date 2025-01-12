using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase

  {
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
      _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
      var result = await _authService.RegisterAsync(request.Email, request.Password, request.Name);
      if (!result.Success)
      {
        return BadRequest(result.Error);
      }
      return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
      var result = await _authService.LoginAsync(request.Email, request.Password);
      if (!result.Success)
      {
        return BadRequest(result.Error);
      }
      return Ok(result);
    }
    
    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
      var result = await _authService.RefreshTokenAsync(request.RefreshToken);
      if (!result.Success)
      {
        return BadRequest(result.Error);
      }
      return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
      var result = await _authService.RequestPasswordResetAsync(request.Email);
      return Ok(new { success = result });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
      var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
      if (!result)
      {
        return BadRequest("Invalid or expired reset token");
      }
      return Ok(new { success = true });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request)
    {
      var result = await _authService.RevokeTokenAsync(request.RefreshToken);
      return Ok(new { success = result });
    }
  }
}