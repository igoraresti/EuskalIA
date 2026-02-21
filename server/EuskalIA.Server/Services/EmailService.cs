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

        public async Task SendVerificationEmailAsync(string email, string username, string token, string language)
        {
            if (_useMockService)
            {
                // Development mode: Print to console
                await SendMockEmailAsync(email, username, token, language);
            }
            else
            {
                // Production mode: Send real email
                await SendRealEmailAsync(email, username, token, language);
            }
        }

        public async Task SendDeactivationEmailAsync(string email, string username, string token)
        {
            if (_useMockService)
            {
                await SendMockDeactivationEmailAsync(email, username, token);
            }
            else
            {
                // Future: Send real email
                await SendMockDeactivationEmailAsync(email, username, token);
            }
        }

        private Task SendMockEmailAsync(string email, string username, string token, string language)
        {
            var verificationUrl = $"http://localhost:5235/api/users/verify-email?token={token}";
            
            string subject, greeting, registrationThanks, clickLink, linkExpiries, ignoreIfWrong;

            switch (language.ToLower())
            {
                case "en":
                    subject = "Verify your EuskalIA account";
                    greeting = $"Hello {username},";
                    registrationThanks = "Thank you for registering on EuskalIA.";
                    clickLink = "Please click on the following link to verify your account:";
                    linkExpiries = "This link will expire in 24 hours.";
                    ignoreIfWrong = "If you have not created this account, you can ignore this message.";
                    break;
                case "eu":
                    subject = "Egiaztatu zure EuskalIA kontua";
                    greeting = $"Kaixo {username},";
                    registrationThanks = "Eskerrik asko EuskalIAn erregistratzeagatik.";
                    clickLink = "Mesedez, egin klik hurrengo estekan zure kontua egiaztatzeko:";
                    linkExpiries = "Esteka hau 24 ordutan iraungiko da.";
                    ignoreIfWrong = "Kontu hau sortu ez baduzu, mezu hau baztertu dezakezu.";
                    break;
                case "pl":
                    subject = "Zweryfikuj swoje konto EuskalIA";
                    greeting = $"Witaj {username},";
                    registrationThanks = "Dziękujemy za rejestrację w EuskalIA.";
                    clickLink = "Kliknij poniższy link, aby zweryfikować swoje konto:";
                    linkExpiries = "Ten link wygaśnie za 24 godziny.";
                    ignoreIfWrong = "Jeśli nie utworzyłeś tego konta, możesz zignorować tę wiadomość.";
                    break;
                case "fr":
                    subject = "Vérifiez votre compte EuskalIA";
                    greeting = $"Bonjour {username},";
                    registrationThanks = "Merci de vous être inscrit sur EuskalIA.";
                    clickLink = "Veuillez cliquer sur le lien suivant pour vérifier votre compte :";
                    linkExpiries = "Ce lien expirera dans 24 heures.";
                    ignoreIfWrong = "Si vous n'avez pas créé ce compte, vous pouvez ignorer ce message.";
                    break;
                case "es":
                default:
                    subject = "Verifica tu cuenta de EuskalIA";
                    greeting = $"Hola {username},";
                    registrationThanks = "Gracias por registrarte en EuskalIA.";
                    clickLink = "Por favor, haz clic en el siguiente enlace para verificar tu cuenta:";
                    linkExpiries = "Este enlace expirará en 24 horas.";
                    ignoreIfWrong = "Si no has creado esta cuenta, puedes ignorar este mensaje.";
                    break;
            }

            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] (Lang: {language}) To: {email}");
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

        private async Task SendRealEmailAsync(string email, string username, string token, string language)
        {
            // TODO: Implement real SMTP email sending using MailKit
            // For now, fall back to mock
            await SendMockEmailAsync(email, username, token, language);
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
