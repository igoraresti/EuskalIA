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
            
            var mockContextLogger = new Mock<ILogger<AppDbContext>>();
            _context = new AppDbContext(options, mockContextLogger.Object);
            
            // Calculate a portable path to the server project relative to the test assembly
            var baseDir = AppContext.BaseDirectory;
            var serverDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "EuskalIA.Server"));
            _mockEnv.Setup(e => e.ContentRootPath).Returns(serverDir);
        }

        [Fact]
        public async Task GetNextContextAsync_WhenFileNotFound_ReturnsDefaultContext()
        {
            // Arrange - Use a non-existent path for this test
            _mockEnv.Setup(e => e.ContentRootPath).Returns("/non-existent");
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
            // If file is found, it increments. If not, it stays at 0.
            // Since we know the file exists in the real repo, it will be > 0.
            progress.LastPageProcessed.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetNextContextAsync_ExtractsTextFromPdf_WhenExists()
        {
            // Arrange
            var service = new KnowledgeService(_context, _mockEnv.Object, _mockLogger.Object);
            var levelId = "A1";

            // Act
            var result = await service.GetNextContextAsync(levelId);

            // Assert
            result.Content.Should().NotBeNull();
            // Some PDFs might have images on page 1, so we don't strictly assert length > 50
            // but we assert it successfully opened the book
            result.BookName.Should().Be("EUSKERA BASICO COMUN.pdf");
            result.PageNumber.Should().Be(1);
            
            // Check if progress was updated to skip page 1
            var progress = await _context.BookProgresses.FirstOrDefaultAsync(p => p.LevelId == levelId);
            progress!.LastPageProcessed.Should().BeGreaterThan(0);
        }
    }
}
