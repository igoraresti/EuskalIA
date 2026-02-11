using Microsoft.Extensions.Configuration;

namespace EuskalIA.Server.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly bool _useMockService;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _useMockService = _configuration.GetValue<bool>("EmailSettings:UseMockService", true);
        }

        public async Task SendVerificationEmailAsync(string email, string username, string token)
        {
            if (_useMockService)
            {
                // Development mode: Print to console
                await SendMockEmailAsync(email, username, token);
            }
            else
            {
                // Production mode: Send real email
                await SendRealEmailAsync(email, username, token);
            }
        }

        private Task SendMockEmailAsync(string email, string username, string token)
        {
            var verificationUrl = $"http://localhost:5235/api/users/verify-email?token={token}";
            
            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] To: {email}");
            Console.WriteLine($"[EMAIL SIMULATION] Subject: Verifica tu cuenta de EuskalIA");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Hola {username},");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Gracias por registrarte en EuskalIA.");
            Console.WriteLine($"[EMAIL SIMULATION] Por favor, haz clic en el siguiente enlace para verificar tu cuenta:");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {verificationUrl}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Este enlace expirará en 24 horas.");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Si no has creado esta cuenta, puedes ignorar este mensaje.");
            Console.WriteLine("**************************************************");
            
            return Task.CompletedTask;
        }

        private async Task SendRealEmailAsync(string email, string username, string token)
        {
            // TODO: Implement real SMTP email sending using MailKit
            // For now, fall back to mock
            await SendMockEmailAsync(email, username, token);
            
            // Future implementation with MailKit:
            // var smtpServer = _configuration["EmailSettings:SmtpServer"];
            // var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
            // var senderEmail = _configuration["EmailSettings:SenderEmail"];
            // var senderPassword = _configuration["EmailSettings:SenderPassword"];
            // 
            // using var client = new SmtpClient();
            // await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            // await client.AuthenticateAsync(senderEmail, senderPassword);
            // 
            // var message = new MimeMessage();
            // message.From.Add(new MailboxAddress("EuskalIA", senderEmail));
            // message.To.Add(new MailboxAddress(username, email));
            // message.Subject = "Verifica tu cuenta de EuskalIA";
            // message.Body = new TextPart("html") { Text = htmlBody };
            // 
            // await client.SendAsync(message);
            // await client.DisconnectAsync(true);
        }
    }
}
