namespace EuskalIA.Server.Services.Email
{
    public interface IEmailQueue
    {
        ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
        ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
