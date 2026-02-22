using EuskalIA.Server.Services.Email;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class EmailQueueTests
    {
        [Fact]
        public async Task EnqueueAndDequeue_ShouldFollowFIFO()
        {
            // Arrange
            var queue = new EmailQueue();
            var message1 = new EmailMessage { ToEmail = "user1@example.com", Subject = "Subject 1" };
            var message2 = new EmailMessage { ToEmail = "user2@example.com", Subject = "Subject 2" };

            // Act
            await queue.EnqueueAsync(message1);
            await queue.EnqueueAsync(message2);

            var result1 = await queue.DequeueAsync(CancellationToken.None);
            var result2 = await queue.DequeueAsync(CancellationToken.None);

            // Assert
            Assert.Equal(message1.ToEmail, result1.ToEmail);
            Assert.Equal(message2.ToEmail, result2.ToEmail);
        }

        [Fact]
        public async Task DequeueAsync_ShouldWaitUntilMessageIsAvailable()
        {
            // Arrange
            var queue = new EmailQueue();
            var message = new EmailMessage { ToEmail = "delayed@example.com", Subject = "Delayed" };

            // Act: Start dequeue in a task
            var dequeueTask = queue.DequeueAsync(CancellationToken.None);

            // Give it a small delay
            await Task.Delay(100);
            Assert.False(dequeueTask.IsCompleted);

            // Enqueue the message
            await queue.EnqueueAsync(message);

            // Wait for dequeue to finish
            var result = await dequeueTask;

            // Assert
            Assert.Equal(message.ToEmail, result.ToEmail);
            Assert.True(dequeueTask.IsCompletedSuccessfully);
        }
    }
}
