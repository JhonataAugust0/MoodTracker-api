using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase

  {
    private readonly IAuthenticationService _authService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthenticationService authService, IEmailService emailService)
    {
      _authService = authService;
      _emailService = emailService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
      var result = await _authService.LoginAsync(request.Email, request.Password);
      if (!result.Success)
      {
        return BadRequest(result.Error);
      }

      await _emailService.SendAccounAccessedEmailAsync(request.Email);
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
    
    [HttpDelete("session")]
    [Authorize] 
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request)
    {
      var result = await _authService.RevokeTokenAsync(request.RefreshToken);
      return Ok(new { success = result });
    }
  }
}