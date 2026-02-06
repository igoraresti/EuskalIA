using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class LeaderboardControllerTests : TestBase
    {
        [Fact]
        public async Task GetWorldLeaderboard_ReturnsTopUsers()
        {
            // Arrange
            var context = GetDatabaseContext();
            for (int i = 1; i <= 15; i++)
            {
                var user = new User { Id = i, Username = $"User{i}" };
                context.Users.Add(user);
                context.Progresses.Add(new Progress { UserId = i, XP = i * 100, WeeklyXP = i * 10, MonthlyXP = i * 50 });
            }
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetWorldLeaderboard("all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Equal(10, list.Count());
        }

        [Fact]
        public async Task GetUserLeaderboard_ReturnsRelativeRanking()
        {
            // Arrange
            var context = GetDatabaseContext();
            for (int i = 1; i <= 20; i++)
            {
                var user = new User { Id = i, Username = $"User{i}" };
                context.Users.Add(user);
                context.Progresses.Add(new Progress { UserId = i, XP = i * 100 });
            }
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetUserLeaderboard(10, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.True(list.Count() <= 11);
        }

        [Fact]
        public async Task GetWorldLeaderboard_ReturnsTopUsers_Weekly()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "User1" });
            context.Progresses.Add(new Progress { UserId = 1, WeeklyXP = 500 });
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetWorldLeaderboard("week");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task GetWorldLeaderboard_ReturnsTopUsers_Monthly()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "User1" });
            context.Progresses.Add(new Progress { UserId = 1, MonthlyXP = 1500 });
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetWorldLeaderboard("month");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task GetUserLeaderboard_Weekly()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "User1" });
            context.Progresses.Add(new Progress { UserId = 1, WeeklyXP = 500 });
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetUserLeaderboard(1, "week");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserLeaderboard_Monthly()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "User1" });
            context.Progresses.Add(new Progress { UserId = 1, MonthlyXP = 1500 });
            await context.SaveChangesAsync();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetUserLeaderboard(1, "month");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserLeaderboard_ReturnsNotFound_WhenUserMissing()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new LeaderboardController(context);

            // Act
            var result = await controller.GetUserLeaderboard(999, "all");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
