using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.AI;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EuskalIA.Tests.Services
{
    public class KnowledgeServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<KnowledgeService>> _mockLogger;
        private readonly AppDbContext _context;

        public KnowledgeServiceTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<KnowledgeService>>();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _mockEnv.Setup(e => e.ContentRootPath).Returns("/tmp");
        }

        [Fact]
        public async Task GetNextContextAsync_WhenFileNotFound_ReturnsDefaultContext()
        {
            // Arrange
            var service = new KnowledgeService(_context, _mockEnv.Object, _mockLogger.Object);
            var levelId = "A1";

            // Act
            var result = await service.GetNextContextAsync(levelId);

            // Assert
            result.Content.Should().Contain("No context available");
            result.BookName.Should().Be("None");
            
            // Check if progress was created in DB
            var progress = await _context.BookProgresses.FirstOrDefaultAsync(p => p.LevelId == levelId);
            progress.Should().NotBeNull();
        }

        [Fact]
        public async Task GetNextContextAsync_InitializesProgressForNewLevel()
        {
            // Arrange
            var service = new KnowledgeService(_context, _mockEnv.Object, _mockLogger.Object);
            var levelId = "B1";

            // Act
            await service.GetNextContextAsync(levelId);

            // Assert
            var progress = await _context.BookProgresses.FirstOrDefaultAsync(p => p.LevelId == levelId);
            progress.Should().NotBeNull();
            progress!.LevelId.Should().Be("B1");
            progress.LastPageProcessed.Should().Be(0);
        }
    }
}
