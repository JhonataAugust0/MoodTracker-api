using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Infrastructure.Adapters;


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

public class Smtp : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public Smtp(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;

        ValidateSettings();
    }
    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
            throw new InvalidOperationException("SMTP server is not configured.");
        if (_emailSettings.SmtpPort <= 0)
            throw new InvalidOperationException("SMTP port is invalid.");
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername))
            throw new InvalidOperationException("SMTP username is not configured.");
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
            throw new InvalidOperationException("SMTP password is not configured.");
        if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            throw new InvalidOperationException("FromEmail is not configured.");
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        try
        {
            using var client = CreateSmtpClient();
            using var message = CreatePasswordResetEmail(toEmail, resetToken);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to send password reset email", ex);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
            EnableSsl = _emailSettings.UseSSL
        };
    }

    private MailMessage CreatePasswordResetEmail(string toEmail, string resetToken)
    {
        var resetLink = $"/reset-password?token={WebUtility.UrlEncode(resetToken)}";

        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = "Password Reset Request",
            IsBodyHtml = true,
            Body = GeneratePasswordResetEmailBody(resetLink)
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