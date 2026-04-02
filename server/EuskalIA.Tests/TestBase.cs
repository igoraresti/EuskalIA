using EuskalIA.Server.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EuskalIA.Tests
{
    public abstract class TestBase
    {
        protected AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockLogger = new Mock<ILogger<AppDbContext>>();
            var context = new AppDbContext(options, mockLogger.Object);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
