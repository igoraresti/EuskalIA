using System.Threading.Channels;

namespace EuskalIA.Server.Services.Email
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<EmailMessage>(options);
        }

        public async ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            await _queue.Writer.WriteAsync(message, cancellationToken);
        }

        public async ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
