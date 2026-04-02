using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;
namespace EuskalIA.Server.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IStringLocalizer<EmailService> _localizer;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEmailQueue emailQueue, IStringLocalizer<EmailService> localizer, IWebHostEnvironment env, ILogger<EmailService> logger)
        {
            _emailQueue = emailQueue;
            _localizer = localizer;
            _env = env;
            _logger = logger;
        }

        public async Task SendVerificationEmailAsync(string email, string username, string token, string language)
        {
            _logger.LogInformation("Preparing verification email for {Email} ({Username}) in language {Language}.", email, username, language);
            // Temporarily switch culture to get correct localized strings
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            
            try
            {
                var culture = new CultureInfo(language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                var verificationUrl = $"http://localhost:8081/verify?token={token}";
                var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Emails", "VerificationTemplate.html");
                var htmlBody = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders with localized strings
                htmlBody = htmlBody
                    .Replace("{{Title}}", _localizer["VerifySubject"])
                    .Replace("{{Greeting}}", string.Format(_localizer["VerifyGreeting"], username))
                    .Replace("{{Message}}", _localizer["VerifyThanks"] + " " + _localizer["VerifyClickLink"])
                    .Replace("{{VerificationUrl}}", verificationUrl)
                    .Replace("{{ButtonText}}", "Verificar Cuenta") // Could also be localized, using a hardcoded localized string for brevity or add to resx
                    .Replace("{{LinkExpiring}}", _localizer["VerifyLinkExpiries"])
                    .Replace("{{IgnoreMessage}}", _localizer["VerifyIgnoreIfWrong"])
                    .Replace("{{Year}}", DateTime.Now.Year.ToString());

                // Fallback for button inside resx logic if not exists. I'll use a direct string for now
                if(language == "en") htmlBody = htmlBody.Replace("Verificar Cuenta", "Verify Account");
                if(language == "eu") htmlBody = htmlBody.Replace("Verificar Cuenta", "Kontua Egiaztatu");

                var message = new EmailMessage
                {
                    ToEmail = email,
                    ToName = username,
                    Subject = _localizer["VerifySubject"],
                    HtmlBody = htmlBody,
                    Language = language
                };

                await _emailQueue.EnqueueAsync(message);
                _logger.LogInformation("Verification email for {Email} enqueued successfully.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing/enqueuing verification email for {Email}.", email);
                throw;
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }

        public async Task SendDeactivationEmailAsync(string email, string username, string token)
        {
            _logger.LogInformation("Preparing deactivation email for {Email} ({Username}).", email, username);
            var language = CultureInfo.CurrentUICulture.Name.Substring(0, 2); // Get current requested language

            var deactivationUrl = $"http://localhost:8081/confirm-deactivation?token={token}";
            var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Emails", "DeactivationTemplate.html");
            var htmlBody = await File.ReadAllTextAsync(templatePath);

            htmlBody = htmlBody
                .Replace("{{Title}}", "Desactivar Cuenta EuskalIA")
                .Replace("{{Greeting}}", $"Hola {username},")
                .Replace("{{Message}}", "Hemos recibido una solicitud para desactivar tu cuenta. Para confirmar esta acción, haz clic en el botón de abajo.")
                .Replace("{{DeactivationUrl}}", deactivationUrl)
                .Replace("{{ButtonText}}", "Confirmar Desactivación")
                .Replace("{{LinkExpiring}}", "Este enlace expirará en 24 horas.")
                .Replace("{{IgnoreMessage}}", "Si no has solicitado esto, puedes ignorar este mensaje de forma segura.")
                .Replace("{{Year}}", DateTime.Now.Year.ToString());

            var message = new EmailMessage
            {
                ToEmail = email,
                ToName = username,
                Subject = "Confirma la desactivación de tu cuenta de EuskalIA",
                HtmlBody = htmlBody,
                Language = language
            };

            await _emailQueue.EnqueueAsync(message);
            _logger.LogInformation("Deactivation email for {Email} ({Username}) enqueued successfully.", email, username);
        }
    }
}
