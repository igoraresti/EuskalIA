using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using Microsoft.AspNetCore.Authorization;
using EuskalIA.Server.Services.Encryption;

namespace EuskalIA.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public AdminController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<PaginatedList<AdminUserDto>>> GetUsers([FromQuery] AdminUserFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.Progress)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            if (filter.JoinedFrom.HasValue)
            {
                query = query.Where(u => u.JoinedAt >= filter.JoinedFrom.Value);
            }

            if (filter.JoinedTo.HasValue)
            {
                query = query.Where(u => u.JoinedAt <= filter.JoinedTo.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.JoinedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = _encryptionService.Decrypt(u.Email),
                    Role = u.Role,
                    IsActive = u.IsActive,
                    IsVerified = u.IsVerified,
                    JoinedAt = u.JoinedAt,
                    XP = u.Progress != null ? u.Progress.XP : 0
                })
                .ToListAsync();

            return Ok(new PaginatedList<AdminUserDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var today = DateTime.UtcNow.Date;
            var registrationsToday = await _context.Users.CountAsync(u => u.JoinedAt >= today);

            return Ok(new AdminStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                RegistrationsToday = registrationsToday
            });
        }

        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Prevent self-deactivation of admin if needed, but let's keep it simple for now
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { isActive = user.IsActive });
        }
    }
}
