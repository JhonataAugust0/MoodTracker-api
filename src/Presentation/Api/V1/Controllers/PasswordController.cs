using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
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

    [HttpPost("forgot"),HttpOptions("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
      try
      {
        var result = await _passwordService.RequestPasswordResetAsync(request.Email);
        if (!result)
          return BadRequest(new { error = "Não foi possível processar sua solicitação" });
        return Ok(new { success = result, message = "Email de recuperação enviado com sucesso" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    [HttpPost("change"),HttpOptions("change")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
    {
      try
      {
        var result = await _passwordService.ChangePasswordAsync(request.Email, request.Token, request.Password, request.NewPassword);
        if (!result)
          return Unauthorized(new { success = false, message = "Usuário não encontrado ou token inválido" });
        return Ok(new { success = true });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }
  }
}