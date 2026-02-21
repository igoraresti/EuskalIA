using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Globalization;
using EuskalIA.Server.Services.Interfaces;

namespace EuskalIA.Server.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<EmailService> _localizer;
        private readonly bool _useMockService;

        public EmailService(IConfiguration configuration, IStringLocalizer<EmailService> localizer)
        {
            _configuration = configuration;
            _localizer = localizer;
            _useMockService = _configuration.GetValue<bool>("EmailSettings:UseMockService", true);
        }

        public async Task SendVerificationEmailAsync(string email, string username, string token, string language)
        {
            // IMPORTANT: In a real app we might use a dedicated LocalizationService 
            // but here we manually switch CultureInfo for the scope of this email.
            var culture = new CultureInfo(language);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            if (_useMockService)
            {
                await SendMockEmailAsync(email, username, token);
            }
            else
            {
                await SendRealEmailAsync(email, username, token);
            }
        }

        public async Task SendDeactivationEmailAsync(string email, string username, string token)
        {
            // For now deactivation emails use current culture (detected by middleware)
            if (_useMockService)
            {
                await SendMockDeactivationEmailAsync(email, username, token);
            }
            else
            {
                await SendMockDeactivationEmailAsync(email, username, token);
            }
        }

        private Task SendMockEmailAsync(string email, string username, string token)
        {
            var verificationUrl = $"http://localhost:5235/api/users/verify-email?token={token}";
            
            var subject = _localizer["VerifySubject"];
            var greeting = string.Format(_localizer["VerifyGreeting"], username);
            var registrationThanks = _localizer["VerifyThanks"];
            var clickLink = _localizer["VerifyClickLink"];
            var linkExpiries = _localizer["VerifyLinkExpiries"];
            var ignoreIfWrong = _localizer["VerifyIgnoreIfWrong"];

            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] To: {email}");
            Console.WriteLine($"[EMAIL SIMULATION] Subject: {subject}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {greeting}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {registrationThanks}");
            Console.WriteLine($"[EMAIL SIMULATION] {clickLink}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {verificationUrl}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {linkExpiries}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {ignoreIfWrong}");
            Console.WriteLine("**************************************************");
            
            return Task.CompletedTask;
        }

        private async Task SendRealEmailAsync(string email, string username, string token)
        {
            await SendMockEmailAsync(email, username, token);
        }

        private Task SendMockDeactivationEmailAsync(string email, string username, string token)
        {
            var deactivationUrl = $"http://localhost:5235/api/users/confirm-deactivation?token={token}";
            
            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] To: {email}");
            Console.WriteLine($"[EMAIL SIMULATION] Subject: Confirma la desactivación de tu cuenta de EuskalIA");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Hola {username},");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Hemos recibido una solicitud para desactivar tu cuenta.");
            Console.WriteLine($"[EMAIL SIMULATION] Para confirmar esta acción, haz clic en el siguiente enlace:");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] {deactivationUrl}");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Este enlace expirará en 24 horas.");
            Console.WriteLine($"[EMAIL SIMULATION]");
            Console.WriteLine($"[EMAIL SIMULATION] Si no has solicitado esto, puedes ignorar este mensaje.");
            Console.WriteLine("**************************************************");
            
            return Task.CompletedTask;
        }
    }
}
