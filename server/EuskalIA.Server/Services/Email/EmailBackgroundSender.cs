using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace EuskalIA.Server.Services.Email
{
    public class EmailBackgroundSender : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<EmailBackgroundSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _useMockService;

        public EmailBackgroundSender(
            IEmailQueue emailQueue, 
            ILogger<EmailBackgroundSender> logger,
            IConfiguration configuration)
        {
            _emailQueue = emailQueue;
            _logger = logger;
            _configuration = configuration;
            _useMockService = _configuration.GetValue<bool>("EmailSettings:UseMockService", true);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Sender started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var emailMessage = await _emailQueue.DequeueAsync(stoppingToken);

                    if (_useMockService)
                    {
                        await SendMockEmailAsync(emailMessage, stoppingToken);
                    }
                    else
                    {
                        await SendRealEmailAsync(emailMessage, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Execution cancelled, likely application shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email queue");
                }
            }

            _logger.LogInformation("Email Background Sender stopping.");
        }

        private Task SendMockEmailAsync(EmailMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation("\n**************************************************");
            _logger.LogInformation($"[EMAIL SIMULATION] To: {message.ToName} <{message.ToEmail}>");
            _logger.LogInformation($"[EMAIL SIMULATION] Subject: {message.Subject}");
            _logger.LogInformation($"[EMAIL SIMULATION] Language: {message.Language}");
            _logger.LogInformation($"[EMAIL SIMULATION]\n{message.HtmlBody}");
            _logger.LogInformation("**************************************************\n");
            
            return Task.CompletedTask;
        }

        private async Task SendRealEmailAsync(EmailMessage message, CancellationToken stoppingToken)
        {
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = _configuration.GetValue<int>("EmailSettings:SmtpPort", 587);
            var username = _configuration["EmailSettings:SmtpUsername"];
            var password = _configuration["EmailSettings:SmtpPassword"];
            var fromName = _configuration.GetValue<string>("EmailSettings:FromName", "EuskalIA");
            var fromEmail = _configuration.GetValue<string>("EmailSettings:FromEmail", "noreply@euskalia.eus");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            email.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
            email.Subject = message.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message.HtmlBody };

            using var smtp = new SmtpClient();
            try
            {
                if (string.IsNullOrEmpty(host) || host == "smtp.example.com")
                {
                    _logger.LogWarning("Real email sending skipped because SMTP Host is not configured or uses placeholder.");
                    return;
                }

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls, stoppingToken);
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await smtp.AuthenticateAsync(username, password, stoppingToken);
                }
                await smtp.SendAsync(email, stoppingToken);
                _logger.LogInformation($"Real email sent to {message.ToEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send real email to {message.ToEmail}");
            }
            finally
            {
                await smtp.DisconnectAsync(true, stoppingToken);
            }
        }
    }
}

