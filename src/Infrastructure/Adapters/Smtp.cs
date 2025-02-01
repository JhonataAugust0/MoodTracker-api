using System.Net;
using System.Net.Mail;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Infrastructure.Adapters;

public class EmailSettings
{
    public string SmtpHost { get; set; }
    public string SmtpPort { get; set; }
    public string UserEmail { get; set; }
    public string Password { get; set; }
    public string SenderName { get; set; } = "MoodTracker";
}

public class Smtp : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public Smtp(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }

    public async Task<bool> SendPasswordRecoverEmailAsync(string mailTo, string link)
    {
        using (var smtpClient = new SmtpClient(_emailSettings.SmtpHost, int.Parse(_emailSettings.SmtpPort)))
        {
            smtpClient.Credentials = new NetworkCredential(_emailSettings.UserEmail, _emailSettings.Password);
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 60000;

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.UserEmail, _emailSettings.UserEmail),
                Subject = "Pedido de redefinição de senha",
                Body = GeneratePasswordResetEmailBody(link),
                IsBodyHtml = true
            };
            mail.To.Add(mailTo);

            await smtpClient.SendMailAsync(mail); 
            return true;
        }
    }

    public async Task<bool> SendAccounAccessedEmailAsync(string mailTo)
    {
        using (var smtpClient = new SmtpClient(_emailSettings.SmtpHost, int.Parse(_emailSettings.SmtpPort)))
        {
            smtpClient.Credentials = new NetworkCredential(_emailSettings.UserEmail, _emailSettings.Password);
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 60000;

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.UserEmail, _emailSettings.UserEmail),
                Subject = "Sua conta foi acessada",
                Body = GenerateAccountAccessedEmailBody(),
                IsBodyHtml = true
            };
            mail.To.Add(mailTo);

            await smtpClient.SendMailAsync(mail); 
            return true;
        }
    }

    public async Task<bool> SendNotificationEmail(string mailTo)
    {
        using (var smtpClient = new SmtpClient(_emailSettings.SmtpHost, int.Parse(_emailSettings.SmtpPort)))
        {
            smtpClient.Credentials = new NetworkCredential(_emailSettings.UserEmail, _emailSettings.Password);
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 60000;

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.UserEmail, _emailSettings.UserEmail),
                Subject = "Lembrete de Registro de Humor",
                Body = GenerateReminderEmailBody(),
                IsBodyHtml = true
            };
            mail.To.Add(mailTo);

            await smtpClient.SendMailAsync(mail);
            return true;
        }
    }

    private string GenerateAccountAccessedEmailBody()
    {
        return $@"
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Acesso à Conta Detectado</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background: linear-gradient(to bottom, #8a2be2, #d896ff);
                        color: #fff;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 50px auto;
                        padding: 20px;
                        background: white;
                        border-radius: 15px;
                        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                        color: #4a4a4a;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        background: linear-gradient(to right, #8a2be2, #d896ff);
                        border-radius: 15px 15px 0 0;
                        color: white;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 24px;
                    }}
                    .content {{
                        padding: 20px;
                        text-align: center;
                    }}
                    .content p {{
                        margin-bottom: 20px;
                        font-size: 16px;
                    }}
                    .btn {{
                        display: inline-block;
                        padding: 15px 25px;
                        margin: 20px 0;
                        background: linear-gradient(to right, #d896ff, #8a2be2);
                        color: white;
                        text-decoration: none;
                        border-radius: 25px;
                        font-size: 16px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                    }}
                    .btn:hover {{
                        opacity: 0.9;
                    }}
                    .footer {{
                        margin-top: 30px;
                        font-size: 14px;
                        color: #8a2be2;
                        text-align: center;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>Acesso à Conta Detectado</h1>
                    </div>
                    <div class=""content"">
                        <p>Olá,</p>
                        <p>Detectamos um acesso à sua conta no <strong>MoodTracker</strong> em ""{DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")}"". Se foi você que acessou, não é necessário realizar nenhuma ação.</p>
                        <p>Caso não tenha sido você, recomendamos que altere sua senha imediatamente para garantir a segurança da sua conta.</p>
                        <a href=""http://localhost:5173/change-password"" style=""color: black"" class=""btn"">Alterar Senha</a>
                        <p>Se precisar de ajuda ou tiver dúvidas, entre em contato conosco.</p>
                    </div>
                    <div class=""footer"">
                        <p>&copy; 2025 MoodTracker. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
    

    private string GeneratePasswordResetEmailBody(string link)
    {
        return $@"
            <html lang=""en"">
                <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Password Reset</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background: linear-gradient(to bottom, #8a2be2, #d896ff);
                        color: #fff;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 50px auto;
                        padding: 20px;
                        background: white;
                        border-radius: 15px;
                        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                        color: #4a4a4a;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        background: linear-gradient(to right, #8a2be2, #d896ff);
                        border-radius: 15px 15px 0 0;
                        color: white;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 24px;
                    }}
                    .content {{
                        padding: 20px;
                        text-align: center;
                    }}
                    .content p {{
                        margin-bottom: 20px;
                        font-size: 16px;
                    }}
                    .btn {{
                        display: inline-block;
                        padding: 15px 25px;
                        margin: 20px 0;
                        background: linear-gradient(to right, #d896ff, #8a2be2);
                        color: white;
                        text-decoration: none;
                        border-radius: 25px;
                        font-size: 16px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                    }}
                    .btn:hover {{
                        opacity: 0.9;
                    }}
                    .footer {{
                        margin-top: 30px;
                        font-size: 14px;
                        color: #8a2be2;
                        text-align: center;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>Redefinição de Senha</h1>
                    </div>
                    <div class=""content"">
                        <p>Olá,</p>
                        <p>Recebemos uma solicitação para redefinir sua senha no <strong>MoodTracker</strong>. Se foi você que solicitou, clique no botão abaixo para redefinir sua senha.</p>
                        <a href=""{link}"" style=""color: black"" class=""btn"">Redefinir Senha</a>
                        <p>Se você não solicitou a redefinição, entre em contato para avisar sobre esse incidente.</p>
                    </div>
                    <div class=""footer"">
                        <p>&copy; 2025 MoodTacker. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GenerateReminderEmailBody()
    {
        return $@"
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Sentimos sua falta!</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background: linear-gradient(to bottom, #8a2be2, #d896ff);
                        color: #fff;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 50px auto;
                        padding: 20px;
                        background: white;
                        border-radius: 15px;
                        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                        color: #4a4a4a;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        background: linear-gradient(to right, #8a2be2, #d896ff);
                        border-radius: 15px 15px 0 0;
                        color: white;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 24px;
                    }}
                    .content {{
                        padding: 20px;
                        text-align: center;
                    }}
                    .content p {{
                        margin-bottom: 20px;
                        font-size: 16px;
                    }}
                    .btn {{
                        display: inline-block;
                        padding: 15px 25px;
                        margin: 20px 0;
                        background: linear-gradient(to right, #d896ff, #8a2be2);
                        color: white;
                        text-decoration: none;
                        border-radius: 25px;
                        font-size: 16px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                    }}
                    .btn:hover {{
                        opacity: 0.9;
                    }}
                    .footer {{
                        margin-top: 30px;
                        font-size: 14px;
                        color: #8a2be2;
                        text-align: center;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>Sentimos sua falta!</h1>
                    </div>
                    <div class=""content"">
                        <p>Olá,</p>
                        <p>Percebemos que faz alguns dias desde sua última visita ao <strong>MoodTracker</strong>, e gostaríamos de saber como você está.</p>
                        <p>Que tal registrar seu humor hoje e continuar acompanhando seu progresso? Cada registro é um passo para um você melhor 😊.</p>
                        <a href=""http://localhost:5173/login"" style=""color: black"" class=""btn"">Voltar para o MoodTracker</a>
                    </div>
                    <div class=""footer"">
                        <p>&copy; 2025 MoodTracker. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
";
    }
}