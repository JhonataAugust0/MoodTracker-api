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
      if (!result)
        return BadRequest("Não foi possível processar sua solicitação");
      return Ok(new { success = result, message = "Email de recuperação enviado com sucesso"});
    }

    [HttpPost("change")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
    {
      var result = await _passwordService.ChangePasswordAsync(request.Email, request.Token, request.Password, request.NewPassword);
      if (!result)
      {
        return Unauthorized(new {success = false, message = "Usuário não encontrado ou token inválido"});
      }
      return Ok(new { success = true });
    }
    
    [HttpGet("redirect-resetpage")]
    public IActionResult RedirectToResetPage([FromQuery] string token)
    {
      return Redirect($"{Environment.GetEnvironmentVariable("WEB_APP_URL")}/change-password?token={token}");
    }
  }
}