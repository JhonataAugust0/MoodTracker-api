using Application.Services;

namespace Infrastructure.Adapters;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
}

// Infrastructure/Email/EmailService.cs


public class Smtp : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<Smtp> _logger;

    public Smtp(IOptions<EmailSettings> emailSettings, ILogger<Smtp> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        try
        {
            using var client = CreateSmtpClient();
            var message = CreatePasswordResetEmail(toEmail, resetToken);
            
            await client.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw new ApplicationException("Failed to send password reset email", ex);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
            EnableSsl = _emailSettings.UseSSL
        };

        return client;
    }

    private MailMessage CreatePasswordResetEmail(string toEmail, string resetToken)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = "Password Reset Request",
            IsBodyHtml = true,
            Body = GeneratePasswordResetEmailBody(resetToken)
        };

        message.To.Add(toEmail);
        return message;
    }

    private string GeneratePasswordResetEmailBody(string resetToken)
    {
        // Você pode personalizar este HTML conforme necessário
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                    <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                    <p style='margin: 25px 0;'>
                        <a href='http://your-frontend-url/reset-password?token={WebUtility.UrlEncode(resetToken)}'
                           style='background-color: #3498db; color: white; padding: 12px 25px; text-decoration: none; border-radius: 3px;'>
                            Reset Password
                        </a>
                    </p>
                    <p>If you did not request this password reset, please ignore this email or contact support if you have concerns.</p>
                    <p>This link will expire in 1 hour for security reasons.</p>
                    <p style='margin-top: 25px; font-size: 12px; color: #666;'>
                        If you're having trouble clicking the button, copy and paste this URL into your web browser:<br>
                        http://your-frontend-url/reset-password?token={WebUtility.UrlEncode(resetToken)}
                    </p>
                </div>
            </body>
            </html>";
    }
}