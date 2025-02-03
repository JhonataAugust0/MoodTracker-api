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
        
        [HttpPost("login"), HttpOptions("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Email, request.Password);
                if (!result.Success)
                {
                    return BadRequest(result.Error);
                }

                await _emailService.SendAccounAccessedEmailAsync(request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [Authorize]
        [HttpPost("refresh-token"), HttpOptions("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                if (!result.Success)
                {
                    return BadRequest(result.Error);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpDelete("session")]
        [Authorize] 
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request)
        {
            try
            {
                var result = await _authService.RevokeTokenAsync(request.RefreshToken);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
