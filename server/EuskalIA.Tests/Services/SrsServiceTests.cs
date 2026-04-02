using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class SrsServiceTests : TestBase
    {
        [Fact]
        public async Task UpdateSrsNodeAsync_CreatesNewNode_WhenNotExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;
            string topic = "NORK";

            // Act
            await service.UpdateSrsNodeAsync(userId, topic, true);

            // Assert
            var node = await context.UserSrsNodes.FirstOrDefaultAsync(n => n.UserId == userId && n.ConceptId == topic);
            Assert.NotNull(node);
            Assert.Equal(topic, node.ConceptId);
            Assert.Equal(1.0f, node.MasteryLevel);
            Assert.NotNull(node.NextReviewDate);
            Assert.True(node.NextReviewDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task UpdateSrsNodeAsync_UpdatesInterval_OnSuccess()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;
            string topic = "NOR-NORK";

            // First time: creates node, next review in 1 day
            await service.UpdateSrsNodeAsync(userId, topic, true);
            var node = await context.UserSrsNodes.FirstAsync(n => n.UserId == userId && n.ConceptId == topic);
            
            // Simule review completion
            node.LastReviewDate = DateTime.UtcNow.AddDays(-1);
            node.NextReviewDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Act - Second success
            await service.UpdateSrsNodeAsync(userId, topic, true);

            // Assert
            node = await context.UserSrsNodes.FirstAsync(n => n.UserId == userId && n.ConceptId == topic);
            // After 1 day, the next interval should be 6 days (standard SM-2 simplification)
            Assert.NotNull(node.NextReviewDate);
            var diff = (node.NextReviewDate.Value - node.LastReviewDate.Value).TotalDays;
            Assert.InRange(diff, 5.9, 6.1);
        }

        [Fact]
        public async Task UpdateSrsNodeAsync_ResetsInterval_OnFailure()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;
            string topic = "ZEOZER";

            // Set up a node with long interval
            var node = new UserSrsNode
            {
                UserId = userId,
                ConceptId = topic,
                LastReviewDate = DateTime.UtcNow.AddDays(-10),
                NextReviewDate = DateTime.UtcNow,
                RiskFactor = 2.5f,
                MasteryLevel = 4.0f
            };
            context.UserSrsNodes.Add(node);
            await context.SaveChangesAsync();

            // Act
            await service.UpdateSrsNodeAsync(userId, topic, false);

            // Assert
            node = await context.UserSrsNodes.FirstAsync(n => n.UserId == userId && n.ConceptId == topic);
            Assert.Equal(2.3f, node.RiskFactor, 1); // EF decreased from 2.5 by 0.2
            var diff = (node.NextReviewDate.Value - node.LastReviewDate.Value).TotalDays;
            Assert.InRange(diff, 0.9, 1.1); // Reset to 1 day
        }

        [Fact]
        public async Task GetPendingReviewsCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;

            context.UserSrsNodes.AddRange(new List<UserSrsNode>
            {
                new UserSrsNode { UserId = userId, ConceptId = "T1", NextReviewDate = DateTime.UtcNow.AddDays(-1) },
                new UserSrsNode { UserId = userId, ConceptId = "T2", NextReviewDate = DateTime.UtcNow.AddDays(1) },
                new UserSrsNode { UserId = userId, ConceptId = "T3", NextReviewDate = DateTime.UtcNow.AddHours(-2) }
            });
            await context.SaveChangesAsync();

            // Act
            var count = await service.GetPendingReviewsCountAsync(userId);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetDueNodesAsync_ReturnsOnlyDueNodes()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;

            context.UserSrsNodes.AddRange(new List<UserSrsNode>()
            {
                new UserSrsNode { UserId = userId, ConceptId = "Due1", NextReviewDate = DateTime.UtcNow.AddMinutes(-5) },
                new UserSrsNode { UserId = userId, ConceptId = "NotDue1", NextReviewDate = DateTime.UtcNow.AddDays(1) },
                new UserSrsNode { UserId = userId, ConceptId = "Due2", NextReviewDate = null } // Should be due
            });
            await context.SaveChangesAsync();

            // Act
            var dueNodes = await service.GetDueNodesAsync(userId);

            // Assert
            Assert.Equal(2, dueNodes.Count);
            Assert.Contains(dueNodes, n => n.ConceptId == "Due1");
            Assert.Contains(dueNodes, n => n.ConceptId == "Due2");
        }

        [Fact]
        public async Task UpdateSrsNodeAsync_ClampsMasteryAndEase()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<SrsService>>();
            var service = new SrsService(context, mockLogger.Object);
            int userId = 1;

            var node = new UserSrsNode
            {
                UserId = userId,
                ConceptId = "ClampTest",
                MasteryLevel = 4.8f,
                RiskFactor = 2.45f,
                LastReviewDate = DateTime.UtcNow.AddDays(-20),
                NextReviewDate = DateTime.UtcNow
            };
            context.UserSrsNodes.Add(node);
            await context.SaveChangesAsync();

            // Act: Success should clamp mastery at 5.0 and EF at 2.5
            await service.UpdateSrsNodeAsync(userId, "ClampTest", true);

            // Assert
            node = await context.UserSrsNodes.FirstAsync(n => n.ConceptId == "ClampTest");
            Assert.Equal(5.0f, node.MasteryLevel);
            Assert.Equal(2.5f, node.RiskFactor);

            // Act: Failures should clamp at 0.0 and 1.3
            node.RiskFactor = 1.4f;
            node.MasteryLevel = 0.5f;
            await context.SaveChangesAsync();
            await service.UpdateSrsNodeAsync(userId, "ClampTest", false);
            await service.UpdateSrsNodeAsync(userId, "ClampTest", false);

            node = await context.UserSrsNodes.FirstAsync(n => n.ConceptId == "ClampTest");
            Assert.Equal(0.0f, node.MasteryLevel);
            Assert.Equal(1.3f, node.RiskFactor);
        }
    }
}
